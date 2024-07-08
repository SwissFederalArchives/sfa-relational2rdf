using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology.Conversion.ReferenceMeta
{
	public class SingleKeyReferenceMeta : IReferenceMeta
	{
		public IRI ForeignKeyIri { get; init; }
		public IRI TargetRowIri { get; init; }
		public string ForeignKeyColumn { get; init; }
		public IForeignKey ForeignKey { get; init; }

		public IAttribute[] SourceAttributes { get; init; }
		public string GetTargetKey(IRow row) => ((string)row[ForeignKeyColumn])?.IriEscape();
	}
}
