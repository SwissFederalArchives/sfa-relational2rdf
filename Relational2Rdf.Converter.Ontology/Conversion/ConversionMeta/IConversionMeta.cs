using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Ontology.Conversion.ReferenceMeta;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology.Conversion.ConversionMeta
{
	public interface IConversionMeta
	{
		public string TypeName { get; }
		public IRI TypeIri { get; }
		public IRI RowBaseIri { get; }

		public string GetKey(IRow row);
		public IConversionMeta GetNestedMeta(IAttribute attr);
		public IAttribute[] Attributes { get; }
		public FrozenDictionary<IAttribute, AttributeItemInfo> AttributeItemInfos { get; }
		public IReferenceMeta[] References { get;  }
	}
}
