using Relational2Rdf.Converter.Ontology.Conversion.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology
{
	public class OntologySettings
	{
		public static readonly OntologySettings Default = new OntologySettings
		{
			BaseIri = "https://ld.admin.ch/",
			SiardIri = "http://siard.link#",
			TableSettings = new TableConversionSettings()
		};

		public string BaseIri { get; init; }
		public string SiardIri { get; init; }
		public TableConversionSettings TableSettings { get; init; }
	}
}
