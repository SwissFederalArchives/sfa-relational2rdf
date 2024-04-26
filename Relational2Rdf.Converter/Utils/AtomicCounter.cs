using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Utils
{
	public class AtomicCounter
	{
		private int _current;

		public AtomicCounter(int current = 0)
		{
			_current=current;
		}

		public int GetNext() => Interlocked.Increment(ref _current);
	}
}
