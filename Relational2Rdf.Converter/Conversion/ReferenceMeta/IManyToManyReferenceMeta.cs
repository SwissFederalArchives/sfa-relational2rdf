using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Conversion.ReferenceMeta
{
	public interface IManyToManyReferenceMeta
	{
		public IRI SourceTypeIri { get; }
		public IRI TargetTypeIri { get; }

		public IRI SourceToTargetPredicate { get; }
		public IRI TargetToSourcePredicate { get; }

		public string GetSourceKey(IRow row);
		public string GetTargetKey(IRow row);
	}
}
