using AwosFramework.Rdf.Lib.Writer;
using Microsoft.Extensions.Logging;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology
{
	public class OntologyTableConverterFactory : IConverterFactory
	{
		private IRelationalDataSource _dataSource;
		private ITripletWriter _writer;
		private ILoggerFactory _factory;

		public Task<ITableConverter> GetTableConverterAsync(ISchema schema, ITable table)
		{
			var reader = _dataSource.GetReader(schema, table);
			return Task.FromResult<ITableConverter>(new OntologyTableConverter(reader, _writer, _factory));
		}

		public Task InitAsync(ITripletWriter writer, IRelationalDataSource source)
		{
			
		}
	}
}
