using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Relational2Rdf.DataSources.Siard.Common
{
	public abstract class AbstractHasDataSourceRef
	{
		internal AbstractHasDataSourceRef() { }

		[XmlIgnore]
		private IRelationalDataSource __source;
		
		internal void __internal_setDataSource(IRelationalDataSource source)
		{
			__source = source;
		}

		[XmlIgnore]
		internal IRelationalDataSource __internal_dataSource => __source;
	}
}
