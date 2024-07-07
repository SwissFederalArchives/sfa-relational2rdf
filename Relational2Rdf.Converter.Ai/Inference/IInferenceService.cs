using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai.Inference
{
	public interface IInferenceService
	{
		public Task<T> RequestJsonModelAsync<T>(string prompt);
	}
}
