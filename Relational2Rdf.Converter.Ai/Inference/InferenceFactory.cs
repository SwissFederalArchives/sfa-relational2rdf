using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai.Inference
{
	public static class InferenceFactory
	{
		public static IInferenceService GetService(AiConfig config, ILoggerFactory factory)
		{
			switch (config.ServiceType)
			{
				case AiServiceType.OpenAI:
					return new OpenAIInference(config, factory);

				case AiServiceType.Ollama:
					return new OllamaInference(config, factory);

				default:
					throw new NotSupportedException($"Service type {config.ServiceType} is not supported.");
			}
		}
	}
}
