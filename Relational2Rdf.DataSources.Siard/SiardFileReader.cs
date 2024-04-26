using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Common.Core;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Relational2Rdf.DataSources.Siard
{
	public class SiardFileReader : IRelationalReader
	{
		public Task<IRelationalDataSource> ReadAsync(string dataSource)
		{
			//var stream = File.OpenRead(dataSource);
			return Task.FromResult<IRelationalDataSource>(new SiardDataSource(dataSource));
		}
	}
}
