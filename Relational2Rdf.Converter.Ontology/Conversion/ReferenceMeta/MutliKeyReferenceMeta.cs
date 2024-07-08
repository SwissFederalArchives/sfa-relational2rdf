using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology.Conversion.ReferenceMeta
{
	public class MultiKeyReferenceMeta : IReferenceMeta
	{
		public IRI TargetRowIri { get; init; }
		public IRI ForeignKeyIri { get; init; }
		public string[] ForeignKeyColumns { get; init; }
		public IForeignKey ForeignKey { get; init; }

		public IAttribute[] SourceAttributes { get; init; }
		public string GetTargetKey(IRow row) => string.Join("+", ForeignKeyColumns.Select(x => (string)row[x])).IriEscape();

		//public string GetTargetKey(IRow row)
		//{
		//	var res = row[ForeignKeyColumns[0]]?.ToString();
		//	if (res == null)
		//		return null;

		//	for(int i = 1; i < ForeignKeyColumns.Length; i++)
		//	{
		//		var value = row[ForeignKeyColumns[i]];
		//		if (value == null)
		//			return null;

		//		res += $"+{value}";
		//	}

		//	return res.IriEscape();
		//}

	}
}
