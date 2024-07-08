using AwosFramework.Rdf.Lib.Core;
using AwosFramework.Rdf.Lib.Writer;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Utils
{
	public static class Extensions
	{
		public static string IriFriendly(this string @string) => @string.ToLower().Replace(" ", "-");
		public static string IriEscape(this string @string) => Uri.EscapeDataString(@string ?? "\0");
		public static string ConvertToBase64(this Stream stream)
		{
			byte[] bytes;
			using (var memoryStream = new MemoryStream())
			{
				stream.CopyTo(memoryStream);
				bytes = memoryStream.ToArray();
			}

			return Convert.ToBase64String(bytes);
		}

		public static string Replace(this string @string, char[] chars, string replace = "") => string.Join(replace, @string.Split(chars));

		public static string Pad(this string @string, int size, char fillChar = ' ')
		{
			if (@string == null)
				return new string(fillChar, size);

			if (@string.Length > size)
				return @string.Substring(0, size);

			if (@string.Length < size)
				return @string + new string(fillChar, size - @string.Length);

			return @string;
		}


		public static void Merge<K, V>(this Dictionary<K,V> dict, Dictionary<K,V> other, bool overwrite = false)
		{
			foreach (var (key, value) in other)
				if (overwrite || dict.ContainsKey(key) == false)
					dict[key] = value;
		}


		public static bool CanWriteRaw(this CommonType type)
		{
			switch (type)
			{
				case CommonType.Date:
				case CommonType.Time:
				case CommonType.TimeSpan:
				case CommonType.String:
				case CommonType.DateTime:
					return false;

				case CommonType.Boolean:
				case CommonType.Integer:
				case CommonType.Decimal:
					return true;
			}

			return false;
		}

	}
}
