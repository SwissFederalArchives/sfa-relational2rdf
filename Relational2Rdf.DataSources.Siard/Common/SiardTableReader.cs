using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.DataSources.Siard.SiardV2;
using Relational2Rdf.DataSources.Siard.Utils;
using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Relational2Rdf.DataSources.Siard.Common
{
	internal class SiardTableReader : ITableReader
	{
		private static readonly XmlNodeType[] INTERESTING_NODE_TYPES = new[] { XmlNodeType.Attribute, XmlNodeType.Element, XmlNodeType.Text, XmlNodeType.EndElement, XmlNodeType.Attribute };

		private XmlReader _reader;
		private ZipArchive _zip;
		private readonly string _path;
		private RowImplementation _row;
		private bool _disposed = false;
		private ISiardArchive _siard;
		private PooledList<RowBlob> _blobSupply;

		public ISchema Schema { get; init; }
		public ITable Table { get; init; }

		public SiardTableReader(ZipArchive archive, ISiardArchive siard, IRelationalDataSource source, ISiardSchema schema, ISiardTable table)
		{
			_zip = archive;
			_siard = siard;
			_path = $"content/{schema.Folder}/{table.Folder}/";
			var entry = archive.GetEntry($"{_path}{table.Folder}.xml");
			_row = new RowImplementation(source, table.Columns, table.Columns.Cast<IAttributeMeta>());
			_reader = XmlReader.Create(entry.Open());
			_reader.ReadRelevant(INTERESTING_NODE_TYPES);
			_reader.CheckElement("Expected table start", "table");
			_blobSupply = new PooledList<RowBlob>(16);
			Schema = schema;
			Table = table;
		}

		private object ReadNextValue(string name)
		{
			var lob = _reader.GetAttribute("file");
			bool empty = _reader.IsEmptyElement;
			object value = null;
			if (lob == null && empty == false)
			{
				if (_reader.ReadRelevant(INTERESTING_NODE_TYPES))
				{
					empty = _reader.NodeType == XmlNodeType.EndElement;
					if (empty == false)
						value = _reader.Value;
				}
			}
			else
			{
				var file = _reader.GetAttribute("file");
				var lenString = _reader.GetAttribute("length");
				var meta = _row.GetAttributeMeta(name);
				var lobPath = $"{_siard.LobFolder}{meta.LobFolder}{file}";
				var entry = _zip.GetEntry(lobPath);
				if (entry != null)
				{
					long len = long.Parse(lenString);
					var blob = _blobSupply.GetNext();
					blob.Setup(entry, len, meta.MimeType, file);
					value = blob;
				}
			}


			if (empty == false)
				if (_reader.ReadRelevant(INTERESTING_NODE_TYPES))
					_reader.CheckEndElement("expected end of cell");

			return value;
		}

		private void ReadArray(List<object> array, string cellName)
		{
			while (_reader.ReadRelevant(INTERESTING_NODE_TYPES) && _reader.NodeType == XmlNodeType.Element)
				array.Add(ReadNextValue(cellName));

			_reader.CheckEndElement("Expected end of cell");
		}

		private void ReadUdtArray(Func<RowImplementation> rowGetter, string cellName)
		{
			while (_reader.ReadRelevant(INTERESTING_NODE_TYPES) && _reader.NodeType == XmlNodeType.Element)
				ReadRow(rowGetter());

			_reader.CheckEndElement("Expected end of cell");
		}

		private void ReadRow(RowImplementation row)
		{
			while (_reader.ReadRelevant(INTERESTING_NODE_TYPES) && _reader.NodeType == XmlNodeType.Element)
			{
				var cellName = _reader.Name;
				if (row.TryGetAttribute(cellName, out var attr))
				{
					switch (attr.AttributeType)
					{
						case AttributeType.Value:
							row.Write(cellName, ReadNextValue(cellName));
							break;

						case AttributeType.Array:
							ReadArray(row.WriteValueArray(cellName), cellName);
							break;

						case AttributeType.Udt:
							ReadRow(row.WriteUdt(cellName));
							break;

						case AttributeType.UdtArray:
							ReadUdtArray(row.WriteUdtArray(cellName), cellName);
							break;
					}
				}
			}
			_reader.CheckEndElement("expected row or udt end");
		}

		public bool ReadNext(out IRow row)
		{
			if (_disposed)
				throw new ObjectDisposedException(nameof(SiardTableReader));

			_blobSupply.Clear();
			if (_reader.ReadRelevant(INTERESTING_NODE_TYPES) == false || _reader.NodeType == XmlNodeType.EndElement)
			{
				row = null;
				return false;
			}

			_row.Clear();
			ReadRow(_row);
			row = _row;
			return true;
		}

		public void Dispose()
		{
			_zip.Dispose();
			_row.Clear();
			_reader.Dispose();
			_disposed = true;
		}
	}
}
