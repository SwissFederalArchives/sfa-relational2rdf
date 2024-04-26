using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.Common
{
	internal interface IAttributeMeta
	{
		public string LobFolder { get; }
		public string MimeType { get; }
		public IEnumerable<IAttributeMeta> Metas { get; }
	}
}
