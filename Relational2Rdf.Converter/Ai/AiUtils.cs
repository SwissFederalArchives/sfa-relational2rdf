using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai
{
	public static class AiUtils
	{
		public static string FindJsonContent(string @string)
		{
			if (@string.Contains("```"))
			{
				var start = @string.IndexOf("```");
				var objStart = @string.IndexOfAny(['{', '['], start);
				var end = @string.IndexOf("```", objStart);
				return @string[objStart..end].Trim();
			}
			else
			{
				var start = @string.IndexOfAny(['{', '[']);
				if (start == -1)
					return @string;

				var end = @string.LastIndexOfAny(['}', ']']);
				return @string[start..(end+1)].Trim();
			}
		}
	}
}
