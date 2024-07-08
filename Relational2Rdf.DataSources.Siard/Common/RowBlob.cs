using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.DataSources.Siard.Utils;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.Common
{
	internal class RowBlob : IBlob, IDisposable
	{
		private ZipArchiveEntry _entry;
		public long Length { get; private set; }
		public string MimeType { get; private set; }
		public string Identifier { get; private set; }
		public Stream GetStream() => _streams.AddAndReturn(_entry.Open());
		private readonly List<Stream> _streams = new List<Stream>();


		public RowBlob()
		{

		}

		public void Setup(ZipArchiveEntry entry, long length, string mimeType, string identifier)
		{
			_entry = entry;
			Length = length;
			MimeType = mimeType;
			Identifier = identifier;
		}

		public void Dispose()
		{
			_streams.ForEach(x => x.Dispose());
			_streams.Clear();
		}
	}
}
