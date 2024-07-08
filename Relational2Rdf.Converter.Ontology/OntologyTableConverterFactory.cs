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
			var reader = _ctx.DataSource.GetReader(schema, table);
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
			var schemas = sub.BeginObjectList(P("hasSchema"));
			foreach (var schema in source.Schemas)
				schemas.Write(WriteSchema(schema));

			sub.EndObjectList(schemas);
			_writer.EndSubject(sub);
		}

		private IRI WriteSchema(ISchema schema)
		{
			var iri = _ctx.GetSchemaIri(schema);
			var sub = _writer.BeginSubject(iri);
			sub.WriteSubjectType(_ctx.SiardIri, "Schema");
			sub.Write(_ctx.NamePredicate, schema.Name);

			var tables = sub.BeginObjectList(P("hasTable"));
			foreach (var table in schema.Tables)
				tables.Write(WriteTable(schema, table));
			
			sub.EndObjectList(tables);
			var types = sub.BeginObjectList(P("hasType"));
			foreach (var type in schema.Types)
				types.Write(WriteType(schema, type));

			sub.EndObjectList(types);
			_writer.EndSubject(sub);
			return iri;
		}

		private IRI WriteType(ISchema schema, IType type)
		{
			var iri = _ctx.GetTypeIri(schema, type);
			var sub = _writer.BeginSubject(iri);
			sub.WriteSubjectType(_ctx.SiardIri, "Type");
			sub.Write(_ctx.NamePredicate, type.Name);
			sub.Write(P("description"), type.Description);
			sub.Write(P("final"), type.Final);
			sub.Write(P("type"), type.Type.ToString());
			sub.Write(P("hasSchema"), _ctx.GetSchemaIri(schema));


			if (type.HasSuperType)
			{
				var superType = _ctx.DataSource.GetSuperType(type, out var superSchema);
				var superIri = _ctx.GetTypeIri(superSchema, superType);
				sub.Write(P("hasSuperType"), superIri);
				_writer.Write(superIri, P("isSuperTypeOf"), iri);
			}

			var attributes = sub.BeginObjectList(P("hasAttributes"));
			foreach(var attr in _ctx.DataSource.GetAllAttributes(type))	
				attributes.Write(WriteAttribute(schema, type, attr));

			sub.EndObjectList(attributes);
			_writer.EndSubject(sub);
			return iri;
		}

		private IRI WriteAttribute(ISchema schema, IType type, IAttribute attribute)
		{
			var iri = _ctx.GetAttributeIri(schema, type, attribute);
			var sub = _writer.BeginSubject(iri);
			sub.WriteSubjectType(_ctx.SiardIri, "Attribute");
			sub.Write(_ctx.NamePredicate, attribute.Name);
			sub.Write(P("description"), attribute.Description);
			sub.Write(P("nullable"), attribute.Nullable);
			sub.Write(P("defaultValue"), attribute.DefaultValue);
			sub.Write(P("sourceType"), attribute.SourceType);
			sub.Write(P("originalSourceType"), attribute.OriginalSourceType);
			HandleUdtType(attribute, iri, sub);
			_writer.EndSubject(sub);
			return iri;
		}


		private IRI WriteTable(ISchema schema, ITable table)
		{
			var iri = _ctx.GetTableIri(schema, table);
			var sub = _writer.BeginSubject(iri);
			sub.WriteSubjectType(_ctx.SiardIri, "Table");
			sub.Write(_ctx.NamePredicate, table.Name);
			sub.Write(P("description"), table.Description);
			sub.Write(P("rows"), table.RowCount);
			sub.Write(P("hasSchema"), _ctx.GetSchemaIri(schema));
			var columns = sub.BeginObjectList(P("hasColumn"));
			foreach (var column in table.Columns)
				columns.Write(WriteColumn(schema, table, column));
			
			sub.EndObjectList(columns);
			var fKeys = sub.BeginObjectList(P("hasForeignKey"));
			foreach(var fKey in table.ForeignKeys)
				fKeys.Write(WriteForeignKey(schema, table, fKey));
		
			sub.EndObjectList(fKeys);
			_writer.EndSubject(sub);
			return iri;
		}


		private IRI WriteForeignKey(ISchema schema, ITable table, IForeignKey key)
		{
			var refSchema = _ctx.DataSource.FindSchema(key.ReferencedSchema);
			var refTable = _ctx.DataSource.FindTable(refSchema, key.ReferencedTable);
			var keyIri = _ctx.GetForeignKeyIri(schema, table, key);
			var sub = _writer.BeginSubject(keyIri);
			sub.WriteSubjectType(_ctx.SiardIri, "ForeignKey");
			sub.Write(_ctx.NamePredicate, key.Name);
			sub.Write(P("referencedSchema"), _ctx.GetSchemaIri(refSchema));
			sub.Write(P("referencedTable"), _ctx.GetTableIri(refSchema, refTable));
			var refCols = sub.BeginObjectList(P("references"));

			foreach (var rerefence in key.References)
				refCols.Write(WriteReference(key, rerefence, keyIri, schema, table));

			sub.EndObjectList(refCols);
			_writer.EndSubject(sub);
			return keyIri;
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

		private void HandleUdtType(IAttribute attribute, IRI attrIri, ISubjectWriter subject)
		{
			if(attribute.AttributeType == AttributeType.Udt || attribute.AttributeType == AttributeType.UdtArray)
			{
				var typeIri = _ctx.GetTypeIri(attribute.UdtSchema, attribute.UdtType);
				subject.Write(P("hasUserDefinedType"), typeIri);
				_writer.Write(typeIri, P("isTypeOf"), attrIri);
			}
		}

		private IRI WriteColumn(ISchema schema, ITable table, IColumn column)
		{
			var iri = _ctx.GetColumnIri(schema, table, column);
			var sub = _writer.BeginSubject(iri);
			sub.WriteSubjectType(_ctx.SiardIri, "Column");
			sub.Write(_ctx.NamePredicate, column.Name);
			sub.Write(P("description"), column.Description);
			sub.Write(P("nullable"), column.Nullable);
			sub.Write(P("defaultValue"), column.DefaultValue);
			sub.Write(P("sourceType"), column.SourceType);
			sub.Write(P("originalSourceType"), column.OriginalSourceType);
			sub.Write(P("cardinality"), column.Cardinality ?? 1);
			HandleUdtType(column, iri, sub);
			_writer.EndSubject(sub);
			return iri;
		}
	}
}
