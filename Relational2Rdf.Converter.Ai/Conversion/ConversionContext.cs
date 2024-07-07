using AwosFramework.Rdf.Lib.Core;
using AwosFramework.Rdf.Lib.Writer;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Ai;
using Relational2Rdf.Converter.Ai.Conversion.Settings;
using Relational2Rdf.Converter.Ai.Inference;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai.Conversion
{
	public class ConversionContext
	{
		public AiConversionSettings Options { get; init; }
		public IRelationalDataSource DataSource { get; init; }
		public IRI BaseIri { get; init; }
		public AiMagic AiMagic { get; init; }
		public ITripletWriter Writer { get; init; }

		private int _nextSchema;
		private int _nextTable;
		private readonly ConcurrentDictionary<string, string> _tablePrefixes = new ConcurrentDictionary<string, string>();
		private readonly ConcurrentDictionary<string, string> _schemaPrefixes = new ConcurrentDictionary<string, string>();
		private readonly ConcurrentDictionary<string, IRI> _tableIris = new ConcurrentDictionary<string, IRI>();
		private readonly ConcurrentDictionary<string, IRI> _tablePredicateIris = new ConcurrentDictionary<string, IRI>();
		private readonly ConcurrentDictionary<string, SchemaContext> _schemaContexts = new ConcurrentDictionary<string, SchemaContext>();

		private readonly IDictionary<string, IRI> _schemaIris;

		public IRI GetSchemaIri(string schemaName) => _schemaIris[schemaName];
		public IRI GetSchemaIri(ISchema schema) => _schemaIris[schema.Name];

		public SchemaContext GetSchemaContext(ISchema schema) => _schemaContexts[schema.Name];
		public SchemaContext GetSchemaContext(string schema) => _schemaContexts[schema];

		public IRI GetTablePredicateIri(ISchema schema, ITable table) => GetTablePredicateIri(schema.Name, table.Name);
		public IRI GetTablePredicateIri(ISchema schema, IType type) => GetTablePredicateIri(schema.Name, type.Name);
		public IRI GetTablePredicateIri(string schema, string table)
		{
			if (_tablePredicateIris.TryGetValue(table, out var iri) == false)
			{
				var tableIri = GetTableIri(schema, table);
				iri = Writer.DefineIri($"{tableIri.Value}predicates#", $"p{tableIri.Prefix}");
				_tablePredicateIris[table] = iri;
			}

			return iri;
		}

		public IRI GetTableIri(ISchema schema, ITable table) => GetTableIri(schema.Name, table.Name);
		public IRI GetTableIri(ISchema schema, IType type) => GetTableIri(schema.Name, type.Name);
		public IRI GetTableIri(string schema, string table)
		{
			if (_tableIris.TryGetValue(table, out var iri) == false)
			{
				var schemaIri = GetSchemaIri(schema);
				var schemaCtx = _schemaContexts[schema];
				iri = Writer.DefineIri($"{schemaIri.Value}{schemaCtx.GetTableName(table)}/", NextTablePrefix(table));
				_tableIris[table] = iri;
			}

			return iri;
		}

		public string NextSchemaPrefix(string schema)
		{
			var next = Interlocked.Increment(ref _nextSchema);
			return $"S{next}";
		}

		public string NextTablePrefix(string table)
		{
			if (_tablePrefixes.TryGetValue(table, out var next) == false)
			{
				next = $"T{Interlocked.Increment(ref _nextTable)}";
				_tablePrefixes[table] = next;
			}

			return next;
		}


		public async Task InitAsync()
		{
			foreach (var schema in DataSource.Schemas)
			{
				var ctx = new SchemaContext(schema, GetSchemaIri(schema));
				_schemaContexts[schema.Name] = ctx;
			}

			await Task.WhenAll(_schemaContexts.Values.Select(x => x.InitAsync(this)));
		}

		public ConversionContext(AiConversionSettings options, AiMagic aiMagic, IRelationalDataSource siardReader, ITripletWriter writer)
		{
			Options = options;
			DataSource = siardReader;
			Writer = writer;
			AiMagic = aiMagic;
			BaseIri = Writer.DefineIri($"{Options.BaseIri}{siardReader.Name.IriFriendly()}/");
			Writer.DefineBase(BaseIri);
			_schemaIris = siardReader.Schemas.ToFrozenDictionary(x => x.Name, x => writer.DefineIri($"{BaseIri.Value}{x.Name.IriFriendly()}/", NextSchemaPrefix(x.Name)));
		}
	}
}
