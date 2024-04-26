using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Core
{
	public interface IRelationalReader
	{
		public Task<IRelationalDataSource> ReadAsync(string dataSource);
	}
}
