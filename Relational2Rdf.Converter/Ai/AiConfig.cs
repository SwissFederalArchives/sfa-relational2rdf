using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai
{
	public class AiConfig
	{
		public string Endpoint { get; init; }
		public string ApiKey { get; init; }
		public string Model { get; init; }
		public AiServiceType ServiceType { get; init; }

		public AiConfig(string endpoint, string apiKey, string model, AiServiceType serviceType)
		{
			Endpoint=endpoint;
			ApiKey=apiKey;
			Model=model;
			ServiceType=serviceType;
		}
	}
}
