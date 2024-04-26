using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Conversion.ReferenceMeta;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Conversion.ConversionMeta
{
	public class NoKeyConversionMeta : IConversionMeta
	{
		public IRI SchemaIri { get; init; }
		public string TypeName { get; init; }
		public IRI BaseIri { get; init; }
		public IAttribute[] ValueAttributes { get; init; }
		public IReferenceMeta[] References { get; init; }
		public IDictionary<IAttribute, IConversionMeta> NestedMetas { get; init; }
		public IDictionary<IAttribute, IRI> PredicateNames { get; init; }
		public AtomicCounter Counter { get; init; }

		public string GetKey(IRow row) => Counter.GetNext().ToString();
		public IConversionMeta GetNestedMeta(IAttribute attr) => NestedMetas[attr];
		public IRI GetPredicate(IAttribute attr) => PredicateNames[attr];
	}
}
