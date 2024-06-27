using Cocona;
using Relational2Rdf.Converter.Conversion.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Relational2Rdf.Cli
{

	public class ConversionParameters : ICommandParameterSet
	{
		[HasDefaultValue]
		[Option("threads", ['t'], Description = "The number of threads to use for processing.")]
		public int ThreadCount { get; set; } = Environment.ProcessorCount;

		[HasDefaultValue]
		[Option("base-iri", ['i'], Description = "The base IRI used for conversion settings.")]
		public string BaseIri { get; set; } = "https://ld.admin.ch/";

		[Option("ai-key", ['k'], Description = "The authentication key for AI services.")]
		public string AiKey { get; set; } = "";

		[HasDefaultValue]
		[Option("ai-endpoint", ['e'], Description = "The endpoint URL for AI services. Must be OpenAI compatible")]
		public string AiEndpoint { get; set; } = "https://api.openai.com/";

		[HasDefaultValue]
		[Option("ai-model", ['m'], Description = "The AI model identifier.")]
		public string AiModel { get; set; } = "gpt-3.5-turbo";

		[HasDefaultValue]
		[Option("ai-service", ['s'], Description = "The AI service to use for inference. Default is OpenAI.")]
		public string AiService { get; set; } = "OpenAI";

		[HasDefaultValue]
		[Option("no-console", Description = "Flag to disable console output.")]
		public bool NoConsoleOutput { get; set; } = false;

		[Option("table-config", ['c'], Description = "The configuration file for table conversion settings.")]
		public string TableConfigPath { get; set; }

		[HasDefaultValue]
		[Option("output", ['o'], Description = "Output directory for converted archive")]
		public DirectoryInfo OutputDirectory { get; set; } = new DirectoryInfo("./");

		[HasDefaultValue]
		[Option("output-file", ['f'], Description = "Output file name for converted archive")]
		public string OutputFileName { get; set; } = null;

		[HasDefaultValue]
		[Option("trace", Description = "Output json file containing time measurements for debugging")]
		public bool Trace { get; set; } = false;

		public ConversionSettings BuildConfig()
		{
			var tableSettings = JsonSerializer.Deserialize<TableConversionSettings>(File.OpenRead(TableConfigPath));
			return new ConversionSettings
			{
				ThreadCount = ThreadCount,
				BaseIri = BaseIri,
				AiKey = AiKey,
				AiEndpoint = AiEndpoint,
				AiModel = AiModel,
				AiService = AiService,
				TableSettings = tableSettings,
				ConsoleOutput = NoConsoleOutput == false,
				FileName = OutputFileName
			};
		}
	}

}
