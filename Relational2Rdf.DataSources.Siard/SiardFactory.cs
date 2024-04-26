using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.DataSources.Siard.Common;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Relational2Rdf.DataSources.Siard
{
	internal static class SiardFactory
	{

		private static SiardVersion DetectMajorVersion(ZipArchive archive)
		{
			var version = archive.Entries.Where(x => x.FullName.StartsWith("header/siardversion/") && x.FullName != "header/siardversion/").FirstOrDefault();
			if (version == null)
				return SiardVersion.Siard1;

			var versionString = version.FullName.Split("/", StringSplitOptions.RemoveEmptyEntries).Last();
			var index = versionString.IndexOf(".");
			if (index > 0)
				versionString = versionString[0..index];

			var major = (SiardVersion)int.Parse(versionString);
			if (Enum.IsDefined(major))
				return major;

			return SiardVersion.Unknown;
		}

		private static readonly IDictionary<SiardVersion, Type> _versionMap = Assembly.GetAssembly(typeof(SiardFactory)).GetTypes()
			.Where(x => x.IsClass && x.IsAssignableTo(typeof(ISiardArchive)))
			.Select(x => (type: x, attr: x.GetCustomAttribute<SiardVersionAttribute>()))
			.Where(x => x.attr != null)
			.ToFrozenDictionary(x => x.attr.Version, x => x.type);

		internal static bool TryGetSiardArchive(ZipArchive archive, out ISiardArchive siard)
		{
			var version = DetectMajorVersion(archive);
			if (_versionMap.TryGetValue(version, out var type))
			{
				var meta = archive.GetEntry("header/metadata.xml");
				using var metaStream = meta.Open();
				var serializer = new XmlSerializer(type);
				siard = (ISiardArchive)serializer.Deserialize(metaStream);
				return true;
			}

			siard = null;
			return false;
		}

		internal static bool TryGetTableReader(ZipArchive archive, ISiardArchive siard, IRelationalDataSource dataSource, ISchema schema, ITable table, out ITableReader reader)
		{
			reader = new SiardTableReader(archive, siard, dataSource, (ISiardSchema)schema, (ISiardTable)table);
			return true;
		}
	}
}
