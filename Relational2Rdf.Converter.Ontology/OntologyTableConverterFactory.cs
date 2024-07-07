using AwosFramework.Rdf.Lib.Core;
using AwosFramework.Rdf.Lib.Writer;
using Microsoft.Extensions.Logging;
using Relation2Rdf.Common.Shims;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology
{
	public class OntologyTableConverterFactory : IConverterFactory
	{
		private IRelationalDataSource _dataSource;
		private ITripletWriter _writer;
		private OntologyConversionContext _ctx;
		private readonly ILoggerFactory _loggerFactory;
		private readonly OntologySettings _settings;

		public OntologyTableConverterFactory(OntologySettings settings, ILoggerFactory loggerFactory)
		{
			_loggerFactory=loggerFactory;
			this._settings=settings;
		}

		public Task<ITableConverter> GetTableConverterAsync(ISchema schema, ITable table)
		{
			var reader = _dataSource.GetReader(schema, table);
			return Task.FromResult<ITableConverter>(new OntologyTableConverter(reader, _writer, _loggerFactory, _ctx, _settings.TableSettings));
		}

		private IRI P(string name) => _ctx.SiardIri.Extend(name);

		public Task InitAsync(ITripletWriter writer, IRelationalDataSource source)
		{
			_ctx = new OntologyConversionContext(writer, source, _settings);
			_writer = writer;
			WriteArchive(source);
			return Task.CompletedTask;
		}

		private void WriteArchive(IRelationalDataSource source)
		{
			var sub = _writer.BeginSubject(_ctx.DataSourceIri);
			sub.WriteSubjectType(_ctx.SiardIri, "Archive");
			sub.Write(P("dbname"), source.Name);
			sub.Write(P("dataOwner"), source.DataOwner);
			sub.Write(P("producerApplication"), source.ProducerApplication);
			foreach (var schema in source.Schemas)
			{
				sub.Write(P("hasSchema"), _ctx.GetSchemaIri(schema));
				WriteSchema(schema);
			}

			_writer.EndSubject(sub);
		}

		private void WriteSchema(ISchema schema)
		{
			var sub = _writer.BeginSubject(_ctx.GetSchemaIri(schema));
			sub.WriteSubjectType(_ctx.SiardIri, "Schema");
			sub.Write(_ctx.NamePredicate, schema.Name);
			var tables = sub.BeginObjectList(_ctx.SiardIri.Extend("hasTable"));
			foreach (var table in schema.Tables)
			{
				tables.Write(_ctx.GetTypeIri(schema, table));
				WriteTable(schema, table);
			}

			sub.EndObjectList(tables);
			_writer.EndSubject(sub);
		}

		private void WriteTable(ISchema schema, ITable table)
		{
			var sub = _writer.BeginSubject(_ctx.GetTypeIri(schema, table));
			sub.WriteSubjectType(_ctx.SiardIri, "Table");
			sub.Write(_ctx.NamePredicate, table.Name);
			sub.Write(P("description"), table.Description);
			sub.Write(P("rows"), table.RowCount);
			var columns = sub.BeginObjectList(P("hasColumn"));
			foreach (var column in table.Columns)
			{
				columns.Write(_ctx.GetColumnIri(schema, table, column));
				WriteColumn(schema, table, column);
			}

			sub.EndObjectList(columns);
			var fKeys = sub.BeginObjectList(P("hasForeignKey"));
			foreach(var fKey in table.ForeignKeys)
			{
				fKeys.Write(_ctx.GetForeignKeyIri(schema, table, fKey));
				WriteForeignKey(schema, table, fKey);
			}

			sub.EndObjectList(fKeys);
			_writer.EndSubject(sub);
		}


		private void WriteForeignKey(ISchema schema, ITable table, IForeignKey key)
		{
			var refSchema = _dataSource.FindSchema(key.ReferencedSchema);
			var refTable = _dataSource.FindTable(refSchema, key.ReferencedTable);
			var keyIri = _ctx.GetForeignKeyIri(schema, table, key);
			var sub = _writer.BeginSubject(keyIri);
			sub.WriteSubjectType(_ctx.SiardIri, "ForeignKey");
			sub.Write(_ctx.NamePredicate, key.Name);
			sub.Write(P("referencedSchema"), _ctx.GetSchemaIri(refSchema));
			sub.Write(P("referencedTable"), _ctx.GetTypeIri(refSchema, refTable));
			var refCols = sub.BeginObjectList(P("references"));

			foreach (var rerefence in key.References)
				refCols.Write(WriteReference(key, rerefence, keyIri, schema, table));

			sub.EndObjectList(refCols);
			_writer.EndSubject(sub);
		}

		private IRI WriteReference(IForeignKey key, IColumnReference reference, IRI keyIri, ISchema schema, ITable table)
		{
			var counter = _ctx.GetCounter<IColumnReference>();
			var refIri = keyIri.Extend($"reference").Extend(counter.GetNext().ToString());	
			var sub = _writer.BeginSubject(refIri);
			sub.WriteSubjectType(_ctx.SiardIri, "ColumnReference");
			var sourceIri = _ctx.GetColumnIri(schema.Name, table.Name, reference.SourceColumn);
			var targetIri = _ctx.GetColumnIri(key.ReferencedSchema, key.ReferencedTable, reference.TargetColumn);
			sub.Write(P("sourceColumn"), sourceIri);
			sub.Write(P("targetColumn"), targetIri);
			sub.Write(P("isOfKey"), keyIri);
			_writer.EndSubject(sub);
			_writer.Write(sourceIri, P("isReferenceSource"), refIri);
			_writer.Write(targetIri, P("isReferenceTarget"), refIri);
			return refIri;
		}

		private void WriteColumn(ISchema schema, ITable table, IColumn column)
		{
			var sub = _writer.BeginSubject(_ctx.GetColumnIri(schema, table, column));
			sub.WriteSubjectType(_ctx.SiardIri, "Column");
			sub.Write(_ctx.NamePredicate, column.Name);
			sub.Write(P("description"), column.Description);
			sub.Write(P("nullable"), column.Nullable);
			sub.Write(P("defaultValue"), column.DefaultValue);
			sub.Write(P("sourceType"), column.SourceType);
			sub.Write(P("originalSourceType"), column.OriginalSourceType);
			_writer.EndSubject(sub);
		}
	}
}
