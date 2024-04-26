using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.Common
{
	internal interface ISiardField : IField
	{
		public IEnumerable<ISiardField> SiardFields { get; }
		public string LobFolder { get; }
	}
}
