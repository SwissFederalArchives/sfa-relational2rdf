using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface IType
	{
		public string Name { get; }
		public bool Final { get; }
		public string Description { get; }
		public bool HasSuperType { get; }
		public TypeType Type { get; }
		public CommonType BaseType { get; }
		public IEnumerable<IAttribute> Attributes { get; }
	}
}
