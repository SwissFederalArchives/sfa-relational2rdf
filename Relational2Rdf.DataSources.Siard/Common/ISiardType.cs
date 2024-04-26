using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.Common
{
	internal interface ISiardType
	{
		public string SuperTypeName { get; }
		public string SuperTypeSchema { get; }
	}
}
