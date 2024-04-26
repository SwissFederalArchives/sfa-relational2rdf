using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface IForeignKey
	{
		public string Name { get; }
		public string ReferencedTable { get; }
		public string ReferencedSchema { get; }
		public IEnumerable<IColumnReference> References { get; }
	}
}
