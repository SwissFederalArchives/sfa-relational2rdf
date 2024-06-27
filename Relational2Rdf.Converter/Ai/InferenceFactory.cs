using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai
{
	public static class InferenceFactory
	{
		public static IInferenceService GetService(AiConfig config)
		{
			switch (config.ServiceType)
			{
				case AiServiceType.OpenAI:
					return new OpenAIInference(config);

				case AiServiceType.Ollama:
					return new OllamaInference(config);

				default:
					throw new NotSupportedException($"Service type {config.ServiceType} is not supported.");
			}
		}
	}
}
