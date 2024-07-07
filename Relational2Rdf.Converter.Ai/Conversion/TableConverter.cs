using AwosFramework.Rdf.Lib.Writer;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Ai.Conversion.ConversionMeta;
using Relational2Rdf.Converter.Ai.Conversion.ReferenceMeta;
using Relational2Rdf.Converter.Ai.Conversion.Settings;
using Relational2Rdf.Converter.Ontology.Conversion.Settings;
using Relational2Rdf.Converter.Utils;
using System.Collections.Frozen;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace Relational2Rdf.Converter.Ai.Conversion
{
	public class TableConverter : ITableConverter
	{
		private ITripletWriter _writer;
		private ITableReader _reader;
		private TableConversionSettings _settings;
		private ConversionContext _ctx;
		private const string PROFILER_CAT_TABLE_CONVERSION = "TableConversion";
		private const string PROFILER_CAT_META_CONSTRUCTION = "MetaConstruction";

		public TableConverter(ConversionContext ctx, ITripletWriter writer, ITableReader reader, TableConversionSettings settings)
		{
			_ctx = ctx;
			_writer = writer;
			_reader = reader;
			_settings = settings;
		}

		public async Task ConvertAsync(IProgress progress)
		{
			if (_settings.ConvertMetadata)
				WriteTypeMetadata();

			var referenceColumns = _reader.Table.ForeignKeys.SelectMany(x => x.References).Select(x => x.SourceColumn).ToFrozenSet();
			var schemaCtx = _ctx.GetSchemaContext(_reader.Schema);
			if (_reader.Table.ForeignKeys.Count() == 2 && _reader.Table.Columns.All(x => referenceColumns.Contains(x.Name)))
			{
				IManyToManyReferenceMeta meta;
				using (Profiler.Trace(PROFILER_CAT_META_CONSTRUCTION, _reader.Table.Name, $"Created meta for {_reader.Table.ColumnNames.Count()} columns"))
				{
					meta = await MetaBuilder.BuildManyToManyReferencesAsync(_ctx, schemaCtx, _reader.Table);
				}

				ConvertManyToMany(_reader, _writer, meta);
			}
			else
			{
				IConversionMeta meta;
				using (Profiler.Trace(PROFILER_CAT_META_CONSTRUCTION, _reader.Table.Name, $"Created meta for {_reader.Table.ColumnNames.Count()} columns"))
				{
					meta = await MetaBuilder.BuildConversionMetaAsync(_ctx, schemaCtx, _reader.Table);
				}

				ConvertTable(_reader, _writer, meta, progress);
			}
		}

		private void ConvertTable(ITableReader reader, ITripletWriter writer, IConversionMeta meta, IProgress progress)
		{
			using (Profiler.Trace(PROFILER_CAT_TABLE_CONVERSION, reader.Table.Name, $"Read and converted {reader.Table.RowCount} rows"))
			{
				while (reader.ReadNext(out var row))
				{
					var key = meta.GetKey(row);
					var subject = writer.BeginSubject(meta.BaseIri, key);
					WriteRow(meta, row, subject);
					WriteReferences(key, meta, row, subject);
					writer.EndSubject(subject);
					progress.Increment();
				}
			}
		}

		private void ConvertManyToMany(ITableReader reader, ITripletWriter writer, IManyToManyReferenceMeta reference)
		{
			using (Profiler.Trace(PROFILER_CAT_TABLE_CONVERSION, reader.Table.Name, $"Read and converted {reader.Table.RowCount} rows"))
			{
				while (reader.ReadNext(out var row))
				{
					var sourceKey = reference.GetSourceKey(row);
					var targetKey = reference.GetTargetKey(row);

					writer.Write(reference.SourceTypeIri, sourceKey, reference.SourceToTargetPredicate, reference.TargetTypeIri, targetKey);
					if (_settings.BiDirectionalReferences)
						writer.Write(reference.TargetTypeIri, targetKey, reference.TargetToSourcePredicate, reference.SourceTypeIri, sourceKey);
				}
			}
		}

		private void WriteTypeMetadata()
		{
		}

		private void WriteReferences(string sourceKey, IConversionMeta meta, IRow row, ISubjectWriter subject)
		{
			foreach (var reference in meta.References)
			{
				var targetKey = reference.GetTargetKey(row);
				if (targetKey != null)
				{
					subject.Write(reference.ForwardPredicate, reference.TargeTypeIri, targetKey);
					if (_settings.BiDirectionalReferences)
						_writer.Write(reference.TargeTypeIri, targetKey, reference.BackwardPredicate, meta.BaseIri, sourceKey);
				}
			}
		}

		private void WriteRow(IConversionMeta meta, IRow row, ISubjectWriter subject)
		{
			subject.WriteSubjectType(meta.SchemaIri, meta.TypeName);
			foreach (var attr in meta.ValueAttributes)
			{
				var value = row[attr.Name];
				if (value != null)
				{
					switch (attr.AttributeType)
					{
						case AttributeType.Value:
							WritePrimitive(meta, attr, value, subject);
							break;

						case AttributeType.Array:
							{
								var list = subject.BeginObjectList(meta.GetPredicate(attr));
								WritePrimitiveArray(attr.CommonType, value as List<object>, list);
								subject.EndObjectList(list);
								break;
							}

						case AttributeType.Udt:
							{
								var nestedMeta = meta.GetNestedMeta(attr);
								var nestedRow = value as IRow;
								var key = nestedMeta.GetKey(nestedRow);
								var sub = _writer.BeginSubject(nestedMeta.BaseIri, key);
								WriteRow(nestedMeta, nestedRow, sub);
								_writer.EndSubject(sub);
								subject.Write(meta.GetPredicate(attr), nestedMeta.BaseIri, key);
								break;
							}

						case AttributeType.UdtArray:
							{
								var list = subject.BeginObjectList(meta.GetPredicate(attr));
								WriteUdtArray(meta.GetNestedMeta(attr), value as List<IRow>, list);
								subject.EndObjectList(list);
								break;
							}
					}
				}
			}
		}

		private void WriteUdtArray(IConversionMeta meta, List<IRow> rows, IObjectListWriter writer)
		{
			foreach (var row in rows)
			{
				var key = meta.GetKey(row);
				var subject = _writer.BeginSubject(meta.BaseIri, key);
				WriteRow(meta, row, subject);
				writer.Write(meta.BaseIri, key);
				_writer.EndSubject(subject);
			}
		}

		private void WritePrimitiveArray(CommonType componentType, List<object> objects, IObjectListWriter writer)
		{
			if (componentType.CanWriteRaw())
			{
				foreach (var obj in objects)
					if (obj != null)
						writer.WriteRaw(obj as string);
			}
			else
			{
				foreach (var obj in objects)
				{
					var data = HandleLargeObject(componentType, obj);
					if (data != null)
						writer.Write(data);
				}
			}
		}

		private string HandleLargeObject(CommonType type, object value)
		{
			if (value == null)
				return null;

			if (value is IBlob blob)
			{
				StreamReader reader = null;
				if (type == CommonType.String)
				{
					var stream = blob.GetStream();
					if (stream == null || stream.CanRead == false)
						return null;

					reader = new StreamReader(stream, leaveOpen: false);
				}
				else
				{
					if (blob.Length > _settings.MaxBlobLength)
						return _settings.BlobToLargeErrorValue;

					var stream = blob.GetStream();
					if (stream == null)
						return null;

					if (blob.Length > _settings.MaxBlobLengthBeforeCompression)
						stream = new GZipStream(stream, _settings.BlobComprresionLevel);

					var crypto = new CryptoStream(stream, new ToBase64Transform(), CryptoStreamMode.Read);
					reader = new StreamReader(crypto, leaveOpen: false);
				}

				var res = reader?.ReadToEnd();
				reader?.Dispose();
				return res;
			}

			return (string)value;
		}

		private void WritePrimitive(IConversionMeta meta, IAttribute attr, object value, ISubjectWriter subject)
		{
			var decodedValue = HandleLargeObject(attr.CommonType, value);
			if (decodedValue == null)
				return;

			if (attr.CommonType.CanWriteRaw())
				subject.WriteRaw(meta.GetPredicate(attr), decodedValue);
			else
				subject.Write(meta.GetPredicate(attr), decodedValue);
		}
	}
}