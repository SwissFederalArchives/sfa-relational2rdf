using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Conversion.ReferenceMeta
{
	public interface IReferenceMeta
	{
		public IRI ForwardPredicate { get; }
		public IRI BackwardPredicate { get; }
		public IRI TargeTypeIri { get; }
		public string GetTargetKey(IRow row);
	}
}
