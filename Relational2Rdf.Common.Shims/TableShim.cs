using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relation2Rdf.Common.Shims
{
	public class TableShim : ITable
	{
		private ITable _base;
		private IEnumerable<IForeignKey> _foreignKeys;

		public TableShim(ITable @base, IEnumerable<IForeignKey> foreignKeys = null)
		{
			_base=@base;
			_foreignKeys=foreignKeys;
		}

		public string Name => _base.Name;

		public string Description => _base.Description;

		public int RowCount => _base.RowCount;

		public IEnumerable<string> ColumnNames => _base.ColumnNames;

		public IEnumerable<IForeignKey> ForeignKeys => ShimHelper.NonNullConcat(_foreignKeys, _base.ForeignKeys).DistinctBy(x => x.Name);

		public IEnumerable<IColumn> Columns => _base.Columns;

		public IEnumerable<IColumn> KeyColumns => _base.KeyColumns;
	}
}
