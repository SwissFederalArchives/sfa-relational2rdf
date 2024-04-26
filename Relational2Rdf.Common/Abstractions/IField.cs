using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface IField
	{
		public string Name { get; }
		public string Description { get; }
		public IEnumerable<IField> Fields { get; }
	}
}
