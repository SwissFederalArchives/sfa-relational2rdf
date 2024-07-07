using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai.Conversion
{
	public class SchemaContext
	{
		public ISchema Schema { get; init; }
		public IRI Iri { get; init; }
		private FrozenDictionary<string, string> _tableNames;
		private readonly Dictionary<string, AtomicCounter> _typeIdCounters = new Dictionary<string, AtomicCounter>();

		public string GetTableName(ITable table) => _tableNames.GetValueOrDefault(table.Name, table.Name);
		public string GetTableName(IType table) => _tableNames.GetValueOrDefault(table.Name, table.Name);
		public string GetTableName(string table) => _tableNames.GetValueOrDefault(table, table);

		private AtomicCounter GetOrCreateCounter(string name)
		{
			if(_typeIdCounters.TryGetValue(name, out var counter) == false)
			{
				counter = new AtomicCounter();
				_typeIdCounters[name] = counter;
			}

			return counter;
		}

		public AtomicCounter GetCounter(IType type) => GetOrCreateCounter(type.Name);
		public AtomicCounter GetCounter(ITable table) => GetOrCreateCounter(table.Name);

		public SchemaContext(ISchema schema, IRI schemaIri)
		{
			Schema = schema;
			Iri = schemaIri;
		}

		public async Task InitAsync(ConversionContext context)
		{
			var names = await context.AiMagic.GetRdfFriendlyNamesAsync(Schema.Tables.Select(x => x.Name).Concat(Schema.Types.Select(x => x.Name)));
			_tableNames = names.ToFrozenDictionary();
		}
	}
}
