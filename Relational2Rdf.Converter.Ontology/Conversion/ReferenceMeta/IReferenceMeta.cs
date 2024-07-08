using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology.Conversion.ReferenceMeta
{
	public interface IReferenceMeta
	{
		public IRI TargetRowIri { get; }
		public IRI ForeignKeyIri { get; }
		public string GetTargetKey(IRow row);
		public IForeignKey ForeignKey { get; }
		public IAttribute[] SourceAttributes { get; }
	}
}
