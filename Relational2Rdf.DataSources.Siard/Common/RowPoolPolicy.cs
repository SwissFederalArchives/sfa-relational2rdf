using Microsoft.Extensions.ObjectPool;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.Common
{
	internal class RowPoolPolicy : IPooledObjectPolicy<RowImplementation>
	{
		private IAttribute[] Attributes { get; init; }
		private IRelationalDataSource Source { get; init; }
		private IAttributeMeta[] AttributeMetas { get; init; }

		public RowPoolPolicy(IAttribute[] attributes, IRelationalDataSource source, IAttributeMeta[] fields)
		{
			Attributes=attributes;
			Source=source;
			AttributeMetas=fields;
		}

		public RowImplementation Create()
		{
			return new RowImplementation(Source, Attributes, AttributeMetas);
		}

		public bool Return(RowImplementation obj)
		{
			obj.Clear();
			return true;
		}
	}
}
