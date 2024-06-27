using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai
{
	public class OllamaInference : IInferenceService
	{
		private const string SystemPrompt = "Always format json with \n```json\n```\n so the parser can understand your output";
		private const int MaxRetries = 3;
		private readonly AiConfig _config;
		private readonly HttpClient _client;
		private readonly JsonSerializerOptions _jsonOptions;

		public OllamaInference(AiConfig config)
		{
			_config=config;
			_client = new HttpClient();
			var endPoint = config.Endpoint;
			endPoint += endPoint.EndsWith("/") ? "api/" : "/api/";
			_client.BaseAddress = new Uri(endPoint);
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config.ApiKey);
			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower
			};
		}

		public record OllamaRequest(string Model, string Prompt, string System = SystemPrompt, bool Stream = false, float Temperature = 0.15F);
		public record OllamaResponse(string Response, bool Done, string Model);

		public async Task<T> RequestJsonModelAsync<T>(string prompt)
		{
			var req = new OllamaRequest(_config.Model, prompt);
			var result = await _client.PostAsJsonAsync("generate", req, _jsonOptions);
			var response = await result.Content.ReadFromJsonAsync<OllamaResponse>();
			var jsonContent = AiUtils.FindJsonContent(response.Response);
			return JsonSerializer.Deserialize<T>(jsonContent);
		}
	}
}
