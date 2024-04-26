using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
	public interface IRelationalDataSource : IDisposable
	{
		public string Name { get; }
		public IEnumerable<ISchema> Schemas { get; }
		public ISchema FindSchema(string name);

		public ITable FindTable(ISchema schema, string table);
		public ITable FindTable(string schema, string table) => FindTable(FindSchema(schema), table);
		public IType FindType(ISchema schema, string type);
		public IType FindType(string schema, string type) => FindType(FindSchema(schema), type);

		public IType GetSuperType(IType type);
		public IEnumerable<IAttribute> GetAllAttributes(IType type);

		public ITableReader GetReader(ISchema schema, ITable table);
	}
}
