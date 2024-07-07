using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter
{
	public class ConverterSettings
	{
		public int ThreadCount { get; set; } = Environment.ProcessorCount;
		public bool ConsoleOutput { get; set; } = true;
		public DirectoryInfo OutputDir { get; set; }
		public string FileName { get; set; } = null;
	}
}
