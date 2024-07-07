using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface IAttribute
	{
		public string Name { get; }
		public string Description { get; }
		public CommonType CommonType { get; }
		public string SourceType { get; }
		public string DefaultValue { get; }
		public string OriginalSourceType { get; }

		public bool Nullable { get; }
		public int? Cardinality { get; }

		public AttributeType AttributeType { get; }

		public string UdtType { get; }
		public string UdtSchema { get; }
	}
}
