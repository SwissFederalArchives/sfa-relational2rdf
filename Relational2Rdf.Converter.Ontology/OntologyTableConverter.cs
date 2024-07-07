using AwosFramework.Rdf.Lib.Core;
using AwosFramework.Rdf.Lib.Writer;
using Microsoft.Extensions.Logging;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Ontology.Conversion.ConversionMeta;
using Relational2Rdf.Converter.Ontology.Conversion.ReferenceMeta;
using Relational2Rdf.Converter.Ontology.Conversion.Settings;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Relational2Rdf.Converter.Ontology
{
	public class OntologyTableConverter : ITableConverter
	{
		private ITableReader _reader;
		private ITripletWriter _writer;
		private ILogger _logger;
		private TableConversionSettings _settings;
		private OntologyConversionContext _ctx;

		public OntologyTableConverter(ITableReader reader, ITripletWriter writer, ILoggerFactory factory, OntologyConversionContext ctx, TableConversionSettings settings)
		{
			_reader=reader;
			_logger = factory.CreateLogger<OntologyTableConverter>();
			_writer=writer;
			_ctx=ctx;
			_settings=settings;
		}

		private IRI P(string name) => _ctx.SiardIri.Extend(name);

		public Task ConvertAsync(IProgress progress)
		{
			_logger.LogInformation("Begin converting table {0}", _reader.Table.Name);
			var tableIri = _ctx.GetTypeIri(_reader.Schema, _reader.Table);
			var meta = MetaBuilder.BuildConversionMeta(_ctx, _reader.Schema, _reader.Table);


			while (_reader.ReadNext(out var row))
			{
				var rowKey = meta.GetKey(row);
				var rowIri = meta.RowBaseIri.Extend(rowKey);
				var sub = _writer.BeginSubject(rowIri);
				WriteRow(meta, row, sub, rowKey, rowIri);
				_writer.EndSubject(sub);
				progress.Increment();
			}

			return Task.CompletedTask;
		}

		private void WriteCellValue(object obj, IConversionMeta meta, IAttribute attr, ISubjectWriter subject)
		{
			switch (attr.AttributeType)
			{
				case AttributeType.Value:
					WritePrimitive(attr, obj, subject);
					break;

				case AttributeType.Array:
					var items = (List<object>)obj;
					var listWriter = subject.BeginObjectList(_ctx.ValuePredicate);
					WritePrimitiveArray(attr.CommonType, items, listWriter);
					subject.EndObjectList(listWriter);
					break;

				case AttributeType.Udt:
					{
						var nestedMeta = meta.GetNestedMeta(attr);
						var nestedRow = obj as IRow;
						var rowKey = nestedMeta.GetKey(nestedRow);
						var rowIri = nestedMeta.RowBaseIri.Extend(rowKey);
						var sub = _writer.BeginSubject(rowIri);
						WriteRow(nestedMeta, nestedRow, sub, rowKey, rowIri);
						_writer.EndSubject(sub);
						subject.Write(_ctx.ValuePredicate, nestedMeta.TypeIri, rowKey);
						break;
					}

				case AttributeType.UdtArray:
					{
						var list = subject.BeginObjectList(_ctx.ValuePredicate);
						WriteUdtArray(meta.GetNestedMeta(attr), obj as List<IRow>, list);
						subject.EndObjectList(list);
						break;
					}
			}
		}

		private void WriteRow(IConversionMeta meta, IRow row, ISubjectWriter subject, string rowKey, IRI rowIri)
		{
			subject.WriteSubjectType(_ctx.SiardIri, "Row");
			var cells = subject.BeginObjectList(P("hasCell"));
			foreach(var (attr, cellName) in meta.AttributeCellNames)
			{
				var cellIri = rowIri.Extend(cellName);
				cells.Write(cellIri);
				var cell = _writer.BeginSubject(cellIri);
				cell.WriteSubjectType(_ctx.SiardIri, "Cell");
				cell.Write(P("hasRow"), rowIri);
				WriteCellValue(row.GetItem(attr.Name), meta, attr, cell);
				_writer.EndSubject(cell);
			}

			foreach(var reference in meta.References)
			{
				var targetKey = reference.GetTargetKey(row);
				if (targetKey == null)
					continue;

				var targetIri = reference.TargeTypeIri.Extend(targetKey);
				var referenceIri = meta.TypeIri.Extend("reference").Extend(rowKey).Extend(HttpUtility.UrlEncode(reference.ForeignKey.Name));
				var sub = _writer.BeginSubject(referenceIri);
				sub.WriteSubjectType(_ctx.SiardIri, "Reference");
				sub.Write(_ctx.ReferencedRowPredicate, targetIri);
				sub.Write(_ctx.IsOfKeyPredicate, reference.ForeignKeyIri);
				_writer.EndSubject(sub);
				foreach(var attr in reference.SourceAttributes)
				{
					var cellIri = rowIri.Extend(meta.AttributeCellNames[attr]);
					_writer.Write(cellIri, _ctx.HasReferencePredicate, referenceIri);
					_writer.Write(referenceIri, _ctx.IsReferencedByPredicate, cellIri);
				}
			}

			subject.EndObjectList(cells);
		}

		private void WriteUdtArray(IConversionMeta meta, List<IRow> rows, IObjectListWriter writer)
		{
			foreach (var row in rows)
			{
				var rowKey = meta.GetKey(row);
				var rowIri = meta.TypeIri.Extend(rowKey);
				var subject = _writer.BeginSubject(rowIri);
				subject.WriteSubjectType(_ctx.SiardIri, meta.TypeName);
				WriteRow(meta, row, subject, rowKey, rowIri);
				writer.Write(rowIri);
				_writer.EndSubject(subject);
			}
		}

		private void WritePrimitive(IAttribute attr, object value, ISubjectWriter subject)
		{
			var decodedValue = HandleLargeObject(attr.CommonType, value);
			if (decodedValue == null)
				return;

			if (attr.CommonType.CanWriteRaw())
				subject.WriteRaw(_ctx.ValuePredicate, decodedValue);
			else
				subject.Write(_ctx.ValuePredicate, decodedValue);
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


	}
}
