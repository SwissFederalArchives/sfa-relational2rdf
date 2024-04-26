using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface ITableReader : IDisposable
	{
		public ISchema Schema { get; }
		public ITable Table { get; }
		public bool ReadNext(out IRow row);
	}
}
