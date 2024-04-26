using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface ISchema
	{
		public string Name { get; }
		public IEnumerable<ITable> Tables { get; }
		public IEnumerable<IType> Types { get; }

		public ITable FindTable(string name) => Tables.FirstOrDefault(x => x.Name == name);
		public IType FindType(string name) => Types.FirstOrDefault(x => x.Name == name);

		}
}
