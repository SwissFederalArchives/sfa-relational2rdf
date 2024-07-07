using AwosFramework.Rdf.Lib.Writer;
using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter
{
	public interface IConverterFactory
	{
		public Task InitAsync(ITripletWriter writer, IRelationalDataSource source);
		public Task<ITableConverter> GetTableConverterAsync(ISchema schema, ITable table);
	}
}
