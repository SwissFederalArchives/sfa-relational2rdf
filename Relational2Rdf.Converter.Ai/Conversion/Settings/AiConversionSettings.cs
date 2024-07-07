using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai.Conversion.Settings
{
	public class AiConversionSettings
	{
		public string BaseIri { get; set; } = "https://ld.admin.ch/";
		public string AiKey { get; set; }
		public string AiEndpoint { get; set; }
		public string AiModel { get; set; }
		public string AiService { get; set; }
		public bool ReconstructMissingRelationships { get; set; } = false;
		public TableConversionSettings TableSettings { get; set; } = new TableConversionSettings();
	}
}
