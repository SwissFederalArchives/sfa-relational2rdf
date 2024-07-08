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
	public class ManyToManyReferenceMeta : IManyToManyReferenceMeta
	{
		public IRI SourceTypeIri { get; init; }
		public IRI TargetTypeIri { get; init; }
		public IRI SourceToTargetPredicate { get; init; }
		public IRI TargetToSourcePredicate { get; init; }

		public string[] SourceColumns { get; init; }
		public string[] TargetColumns { get; init; }

		public string GetSourceKey(IRow row)
		{
			if (SourceColumns.Length == 1)
				return (string)row[SourceColumns[0]];
			else
				return string.Join("+", SourceColumns.Select(x => (string)row[x])).IriEscape();
		}

		public string GetTargetKey(IRow row)
		{
			if (TargetColumns.Length == 1)
				return (string)row[TargetColumns[0]];
			else
				return string.Join("+", TargetColumns.Select(x => (string)row[x])).IriEscape();

		}
	}
}
