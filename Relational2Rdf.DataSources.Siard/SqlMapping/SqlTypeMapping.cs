using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.SqlMapping
{
	public static class SqlTypeMapping
	{
		private static readonly IDictionary<string, CommonType> _map;

		static SqlTypeMapping()
		{
			_map = Mappings.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true)
				.Cast<DictionaryEntry>()
				.ToFrozenDictionary(x => x.Key.ToString().ToUpper(), x => Enum.Parse<CommonType>(x.Value.ToString()));
		}

		public static CommonType GetFromSql(string sqlType)
		{
			if (string.IsNullOrWhiteSpace(sqlType))
				return CommonType.Unknown;

			var index = sqlType.IndexOf("(");
			if (index > 0)
				sqlType = sqlType[0..index];

			if(_map.TryGetValue(sqlType, out CommonType commonTypes))
				return commonTypes;

			return CommonType.Unknown;
		}
	}
}
