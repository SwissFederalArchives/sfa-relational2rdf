using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relation2Rdf.Common.Shims
{
	public static class ShimHelper
	{
		public static IEnumerable<T> NonNullConcat<T>(params IEnumerable<T>[] enumerables)
		{
			return enumerables.Where(x => x != null).SelectMany(x => x);
		}
	}
}
