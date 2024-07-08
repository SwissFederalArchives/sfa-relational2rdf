using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai.Conversion.ReferenceMeta
{
	public class MultiKeyReferenceMeta : IReferenceMeta
	{
		public IRI ForwardPredicate { get; init; }
		public IRI BackwardPredicate { get; init; }
		public IRI TargeTypeIri { get; init; }
		public string[] ForeignKeyColumns { get; init; }

		public string GetTargetKey(IRow row) => string.Join("+", ForeignKeyColumns.Select(x => (string)row[x])).IriEscape();

	}
}
