using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.Common
{
	internal interface ISiardArchive
	{
		public string ProducerApplication { get; }
		public string DataOwner { get; }
		public IEnumerable<ISchema> Schemas { get; }
		public SiardVersion Version { get; }
		public string Name { get; }
		public string Description { get; }
		public string LobFolder { get; }
	}
}
