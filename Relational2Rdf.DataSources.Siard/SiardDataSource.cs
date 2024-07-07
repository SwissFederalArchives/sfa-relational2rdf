using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Frozen;
using Relational2Rdf.DataSources.Siard.Common;

namespace Relational2Rdf.DataSources.Siard
{
	public class SiardDataSource : IRelationalDataSource, IDisposable
	{
		internal ISiardArchive Archive { get; init; }
		private string _path;

		public SiardDataSource(string path)
		{
			_path = path;
			using var zip = new ZipArchive(File.OpenRead(path), ZipArchiveMode.Read, false);
			if (SiardFactory.TryGetSiardArchive(zip, out var archive))
			{
				Archive = archive;
				foreach (var schema in Archive.Schemas)
				{
					foreach (var item in schema.Types.SelectMany(x => x.Attributes).OfType<AbstractHasDataSourceRef>().Concat(schema.Tables.SelectMany(x => x.Columns).OfType<AbstractHasDataSourceRef>()))
						item.__internal_setDataSource(this);
				}
			}
			else
			{
				Dispose();
				throw new ArgumentException("Unknown siard archive type");
			}
		}

		public void Dispose()
		{
			//_zipArchive.Dispose();
		}

		public ISchema FindSchema(string name)
		{
			return Archive.Schemas.FirstOrDefault(x => x.Name == name);
		}

		public ITable FindTable(ISchema schema, string table)
		{
			return schema.FindTable(table);
		}

		public ITableReader GetReader(ISchema schema, ITable table)
		{
			var zip = new ZipArchive(File.OpenRead(_path), ZipArchiveMode.Read, false);
			if (SiardFactory.TryGetTableReader(zip, Archive, this, schema, table, out var reader))
				return reader;

			return null;
		}

		public IType FindType(ISchema schema, string type)
		{
			return schema.Types.FirstOrDefault(x => x.Name == type);
		}

		public IType GetSuperType(IType type)
		{
			// check if implementation stems from a siard archive supporting user defined types
			if (type is ISiardType sType)
			{

				if (string.IsNullOrEmpty(sType.SuperTypeName) || string.IsNullOrEmpty(sType.SuperTypeSchema))
					return null;

				return ((IRelationalDataSource)this).FindType(sType.SuperTypeSchema, sType.SuperTypeName);
			}
				
			throw new ArgumentException($"Siard V1 doesn't support types");
		}

		public IEnumerable<IAttribute> GetAllAttributes(IType type)
		{
			do
			{
				foreach (var attr in type.Attributes)
					yield return attr;

				type = GetSuperType(type);
			} while (type != null);
		}

		public string Name => Archive.Name;
		public IEnumerable<ISchema> Schemas => Archive.Schemas;
		public string ProducerApplication => Archive.ProducerApplication;
		public string DataOwner => Archive.DataOwner;

	}
}
