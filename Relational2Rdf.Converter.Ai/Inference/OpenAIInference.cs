using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Schema;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai.Inference
{
	public class OpenAIInference : IInferenceService
	{
		private readonly OpenAIAPI _api;
		private readonly string _model;
		private ILogger _logger;

		public OpenAIInference(AiConfig config, ILoggerFactory factory)
		{
			_logger = factory.CreateLogger<OpenAIInference>();
			_model = config.Model ?? "gpt-3.5-turbo";
			//_apiKey = config.ApiKey;
			_api = new OpenAIAPI(config.ApiKey);
			if (string.IsNullOrEmpty(config.Endpoint) == false)
				_api.ApiUrlFormat = $"{config.Endpoint}/{{0}}/{{1}}";

			_logger.LogInformation("Created Inference for endpoint {endpoint}, model {model}", config.Endpoint, config.Model);
		}

		public async Task<T> RequestJsonModelAsync<T>(string prompt)
		{
			var req = new ChatRequest { MaxTokens = 2048, Model = _model, Temperature = 0.15F, Messages = new[] { new ChatMessage(ChatMessageRole.User, prompt) } };
			var completion = await _api.Chat.CreateChatCompletionAsync(req);
			var content = completion.Choices.First().Message.TextContent;
			try
			{
				var jsonContent = AiUtils.FindJsonContent(content);
				var response = JsonSerializer.Deserialize<T>(jsonContent);
				_logger.LogDebug("Received response from ai \n<PROMPT>\n{prompt}\n</PROMPT>\n<RESPONSE>\n{response}\n</RESPONSE>", prompt, jsonContent);
				return response;
			}
			catch (JsonException ex)
			{
				_logger.LogError(ex, "Error deserializing json content from ai \n<PROMPT>\n{prompt}\n</PROMPT>\n<RESPONSE>\n{response}\n</RESPONSE>", prompt, content);
				throw;
			}
		}
	}
}
