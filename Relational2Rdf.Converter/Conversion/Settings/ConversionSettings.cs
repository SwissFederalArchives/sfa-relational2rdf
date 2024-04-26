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
		public DirectoryInfo TempDir { get; set; }
		public bool KeepTempOutput { get; set; } = false;
		public string BaseIri { get; set; } = "https://ld.admin.ch/";
		public string AiKey { get; set; }
		public string AiEndpoint { get; set; }
		public string AiModel { get; set; }
		public TableConversionSettings TableSettings { get; set; } = new TableConversionSettings();

		public ConversionSettings()
		{
			TempDir = new DirectoryInfo("./temp");
		}
	}
}
