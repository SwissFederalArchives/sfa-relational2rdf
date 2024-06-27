using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Conversion.Settings
{
	public class ConversionSettings
	{
		public int ThreadCount { get; set; } = Environment.ProcessorCount;
		public string BaseIri { get; set; } = "https://ld.admin.ch/";
		public string AiKey { get; set; }
		public string AiEndpoint { get; set; }
		public string AiModel { get; set; }
		public string AiService { get; set; }
		public string FileName { get; set; } = null;
		public TableConversionSettings TableSettings { get; set; } = new TableConversionSettings();
		public bool ConsoleOutput { get; set; } = true;
	}
}
