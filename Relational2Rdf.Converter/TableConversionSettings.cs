using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology.Conversion.Settings
{
	public class TableConversionSettings
	{
		public long? MaxBlobLength { get; set; } = 1024 * 1024 * 8;
		public long? MaxBlobLengthBeforeCompression { get; set; } = 1024 * 1024;
		public CompressionLevel BlobComprresionLevel { get; set; } = CompressionLevel.SmallestSize;
		public string BlobToLargeErrorValue { get; set; } = null;
		public bool ConvertMetadata { get; set; } = false;
		public bool BiDirectionalReferences { get; set; } = true;
	}
}
