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

		public OpenAIInference(AiConfig config)
		{
			_model = config.Model ?? "gpt-3.5-turbo";
			//_apiKey = config.ApiKey;
			_api = new OpenAIAPI(config.ApiKey);
			if (string.IsNullOrEmpty(config.Endpoint) == false)
				_api.ApiUrlFormat = $"{config.Endpoint}/{{0}}/{{1}}";
		}

	

		public async Task<T> RequestJsonModelAsync<T>(string prompt)
		{
			var req = new ChatRequest { MaxTokens = 2048, Model = _model, Temperature = 0.15F, Messages = new[] { new ChatMessage(ChatMessageRole.User, prompt) } };
			var completion = await _api.Chat.CreateChatCompletionAsync(req);
			var content = completion.Choices.First().Message.TextContent;
			var jsonContent = AiUtils.FindJsonContent(content);
			return JsonSerializer.Deserialize<T>(jsonContent);
		}
	}
}
