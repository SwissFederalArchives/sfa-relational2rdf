using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface IRow
	{
		public IAttribute[] Attributes { get; }
		public object[] Items { get; }
		public object GetItem(string column);
		public object GetItem(int index);
		public IEnumerable<(IAttribute Attribute, object Value)> Enumerate();

		public object this[string col] => GetItem(col); 
	}
}
