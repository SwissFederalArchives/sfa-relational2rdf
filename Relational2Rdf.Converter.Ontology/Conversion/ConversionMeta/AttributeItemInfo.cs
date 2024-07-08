using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology.Conversion.ConversionMeta
{
	public record AttributeItemInfo(IAttribute Attribute, IRI AttributeIri, string CellName)
	{
	}
}
