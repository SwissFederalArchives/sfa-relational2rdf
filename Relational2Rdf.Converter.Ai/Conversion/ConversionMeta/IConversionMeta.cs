using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Ai.Conversion.ReferenceMeta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai.Conversion.ConversionMeta
{
	public interface IConversionMeta
	{
		public IRI SchemaIri { get; }
		public string TypeName { get; }
		public IRI BaseIri { get; }
		public string GetKey(IRow row);
		public IRI GetPredicate(IAttribute attr);
		public IConversionMeta GetNestedMeta(IAttribute attr);
		public IAttribute[] ValueAttributes { get; }
		public IReferenceMeta[] References { get; }
	}
}
