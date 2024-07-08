using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relation2Rdf.Common.Shims
{
	public class RelationalDatasourceShim : IRelationalDataSource
	{
		private IRelationalDataSource _base;
		private IEnumerable<ISchema> _schemas;

		public RelationalDatasourceShim(IRelationalDataSource @base, IEnumerable<ISchema> schemas)
		{
			_base=@base;
			_schemas=schemas;
		}

		public string Name => _base.Name;
		public IEnumerable<ISchema> Schemas => _schemas;
		public string ProducerApplication => _base.ProducerApplication;
		public string DataOwner => _base.DataOwner;

		public void Dispose()
		{
			_base.Dispose();
		}

		public ISchema FindSchema(string name)
		{
			return _schemas.FirstOrDefault(x => x.Name == name);
		}

		public ITable FindTable(ISchema schema, string table)
		{
			return schema.Tables.FirstOrDefault(x => x.Name == table);
		}

		public IType FindType(ISchema schema, string type)
		{
			return schema.Types.FirstOrDefault(x => x.Name == type);
		}

		public IType GetSuperType(IType type, out ISchema schema) => _base.GetSuperType(type, out schema);

		public IEnumerable<IAttribute> GetAllAttributes(IType type) => _base.GetAllAttributes(type);

		public ITableReader GetReader(ISchema schema, ITable table)
		{
			var bSchema = _base.FindSchema(schema.Name);
			var bTable = _base.FindTable(bSchema, table.Name);
			return _base.GetReader(bSchema, bTable);
		}
	}
}

