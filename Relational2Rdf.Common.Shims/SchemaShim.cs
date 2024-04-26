using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relation2Rdf.Common.Shims
{
	public class SchemaShim : ISchema
	{
		private ISchema _base;
		private IEnumerable<ITable> _tables;

		public SchemaShim(ISchema @base, IEnumerable<ITable> tables)
		{
			_base=@base;
			_tables=tables;
		}

		public string Name => _base.Name;

		public IEnumerable<ITable> Tables => _tables;

		public IEnumerable<IType> Types => _base.Types;
	}
}
