using AwosFramework.Rdf.Lib.Core;
using AwosFramework.Rdf.Lib.Writer;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Relational2Rdf.Converter.Ontology
{
	public class OntologyConversionContext
	{
		public IRelationalDataSource DataSource { get; init; }

		public IRI BaseIri { get; init; }
		public IRI SiardIri { get; init; }
		public IRI DataSourceIri { get; init; }
		public FrozenDictionary<string, IRI> SchemaIris { get; init; }
		public FrozenDictionary<(string, string), IRI> TableIris { get; init; }
		public FrozenDictionary<(string, string, string), IRI> ColumnIris { get; init; }
		public FrozenDictionary<(string, string, string), IRI> AttributeIris { get; init; }
		public FrozenDictionary<(string, string, string), IRI> ForeignKeyIris { get; init; }
		public ConcurrentDictionary<string, AtomicCounter> Counters { get; init; } = new ConcurrentDictionary<string, AtomicCounter>();
		
		public IRI NamePredicate { get; init; }
		public IRI ValuePredicate { get; init; }
		public IRI ReferencedRowPredicate { get; init; }
		public IRI IsOfKeyPredicate { get; init; }
		public IRI HasReferencePredicate { get; init; }
		public IRI IsReferencedByPredicate { get; init; }
		public IRI HasRowPredicate { get; init; }
		public IRI HasTablePredicate { get; init; }
		public IRI HasColumnPredicate { get; init; }
		public IRI HasCellPredicate { get; init; }

		public IRI GetSchemaIri(string schemaName) => SchemaIris[schemaName];
		public IRI GetTypeIri(string schemaName, string tableName) => TableIris[(schemaName, tableName)];
		public IRI GetColumnIri(string schemaName, string tableName, string columnName) => ColumnIris[(schemaName, tableName, columnName)];
		public IRI GetForeignKeyIri(string schemaName, string tableName, string foreignKeyName) => ForeignKeyIris[(schemaName, tableName, foreignKeyName)];

		public IRI GetSchemaIri(ISchema schema) => GetSchemaIri(schema.Name);
		public IRI GetTableIri(ISchema schema, ITable table) => GetTypeIri(schema.Name, table.Name);
		public IRI GetTypeIri(ISchema schema, IType type) => GetTypeIri(schema.Name, type.Name);
		public IRI GetColumnIri(ISchema schema, ITable table, IColumn column) => GetColumnIri(schema.Name, table.Name, column.Name);
		public IRI GetForeignKeyIri(ISchema schema, ITable table, IForeignKey key) => GetForeignKeyIri(schema.Name, table.Name, key.Name);

		public AtomicCounter GetCounter(string category) => Counters.GetOrAdd(category, _ => new AtomicCounter());
		public AtomicCounter GetCounter(Type type) => GetCounter(type.FullName ?? type.Name);
		public AtomicCounter GetCounter<T>() => GetCounter(typeof(T));

		public IRI GetAttributeIri(string schema, string type, string attribute) => AttributeIris[(schema, type, attribute)];
		public IRI GetAttributeIri(ISchema schema, IType type, IAttribute attribute) => GetAttributeIri(schema.Name, type.Name, attribute.Name);

		public OntologyConversionContext(ITripletWriter writer, IRelationalDataSource source, OntologySettings settings)
		{
			DataSource = source;
			BaseIri = writer.DefineIri(settings.BaseIri);
			DataSourceIri = BaseIri.Extend(HttpUtility.UrlEncode(source.Name));
			writer.DefineBase(DataSourceIri);
			SiardIri = writer.DefineIri(settings.SiardIri, "siard");
		
			NamePredicate = SiardIri.Extend("name");
			ValuePredicate = SiardIri.Extend("value");
			ReferencedRowPredicate = SiardIri.Extend("hasRereferencedRow");
			IsOfKeyPredicate = SiardIri.Extend("isOfKey");
			HasReferencePredicate = SiardIri.Extend("hasReference");
			IsReferencedByPredicate = SiardIri.Extend("isReferencedBy");
			HasRowPredicate = SiardIri.Extend("hasRow");
			HasTablePredicate = SiardIri.Extend("hasTable");
			HasColumnPredicate = SiardIri.Extend("hasColumn");
			HasCellPredicate = SiardIri.Extend("hasCell");

			SchemaIris = source.Schemas.ToFrozenDictionary(x => x.Name, x => DataSourceIri.Extend(HttpUtility.UrlEncode(x.Name)));
			TableIris = source.Schemas
				.SelectMany(s => s.Tables.Select(t => (table: t.Name, schema: s)))
				.Concat(source.Schemas.SelectMany(s => s.Types.Select(t => (table: t.Name, schema: s))))
				.ToFrozenDictionary(x => (x.schema.Name, x.table), x => SchemaIris[x.schema.Name].Extend(HttpUtility.UrlEncode(x.table)));

			ColumnIris = source.Schemas
				.SelectMany(s => s.Tables.SelectMany(t => t.Columns.Select(c => (schema: s, table: t, column: c))))
				.ToFrozenDictionary(x => (x.schema.Name, x.table.Name, x.column.Name), x => TableIris[(x.schema.Name, x.table.Name)].Extend(Uri.EscapeDataString(x.column.Name)));
		
			AttributeIris = source.Schemas
				.SelectMany(s => s.Types.SelectMany(t => DataSource.GetAllAttributes(t).Select(a => (schema: s, type: t, attribute: a))))
				.ToFrozenDictionary(x => (x.schema.Name, x.type.Name, x.attribute.Name), x => TableIris[(x.schema.Name, x.type.Name)].Extend(Uri.EscapeDataString(x.attribute.Name)));
				
			ForeignKeyIris = source.Schemas
				.SelectMany(s => s.Tables.SelectMany(t => t.ForeignKeys.Select(k => (schema: s, table: t, key: k))))
				.ToFrozenDictionary(x => (x.schema.Name, x.table.Name, x.key.Name), x => TableIris[(x.schema.Name, x.table.Name)].Extend("foreignKey").Extend(Uri.EscapeDataString(x.key.Name)));
		}
	}
}
