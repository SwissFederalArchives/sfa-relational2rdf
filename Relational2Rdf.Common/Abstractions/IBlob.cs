using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface IBlob
	{
		public Stream GetStream();
		public long Length { get; }
		public string MimeType { get; }
	}
}
