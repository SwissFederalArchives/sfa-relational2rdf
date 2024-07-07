using AwosFramework.Rdf.Lib.Writer;
using Microsoft.Extensions.Logging;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology
{
	public class OntologyTableConverter : ITableConverter
	{
		private ITableReader _reader;
		private ITripletWriter _writer;
		private ILogger _logger;

		public OntologyTableConverter(ITableReader reader, ITripletWriter writer, ILoggerFactory factory)
		{
			_reader=reader;
			_logger = factory.CreateLogger<OntologyTableConverter>();
			_writer=writer;
		}

		public Task ConvertAsync(IProgress progress)
		{
			_logger.LogInformation("Begin converting table {0}", _reader.Table.Name);
		}
	}
}
