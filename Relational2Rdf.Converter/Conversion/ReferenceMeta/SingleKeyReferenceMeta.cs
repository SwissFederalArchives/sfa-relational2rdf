using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Conversion.ReferenceMeta
{
	public class SingleKeyReferenceMeta : IReferenceMeta
	{
		public IRI ForwardPredicate { get; init; }
		public IRI BackwardPredicate { get; init; }
		public IRI TargeTypeIri { get; init; }
		public string ForeignKeyColumn { get; init; }

		public string GetTargetKey(IRow row) => (string)row[ForeignKeyColumn];
	}
}
