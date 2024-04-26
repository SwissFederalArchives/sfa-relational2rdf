using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface ITable
	{
		public string Name { get; }
		public string Description { get; }
		public int RowCount { get; }
		public IEnumerable<string> ColumnNames { get; }
		public IEnumerable<IForeignKey> ForeignKeys { get; }
		public IEnumerable<IColumn> Columns { get; }
		public IEnumerable<IColumn> KeyColumns { get; }
	}
}
