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
			Indentation = "  ",
			BaseUri = "https://ld.admin.ch/",
			PredicatePrefix = "http://siard.link#",
			TypePrefix = "http://siard.link#",
			UsePrefixes = false
		};

		public string BaseUri { get; init; }
		public string PredicatePrefix { get; init; }
		public string TypePrefix { get; init; }
		public bool UsePrefixes { get; init; }
		public string Indentation { get; init; } 
		public string LineBreak { get; init; } = "\n";
	}
}
