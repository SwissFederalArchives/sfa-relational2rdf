using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relation2Rdf.Common.Shims
{
	public class ForeignKeyShim : IForeignKey
	{
		private IForeignKey _base;
		private string _name;
		private string _referencedTable;
		private string _referencedSchema;
		private IEnumerable<IColumnReference> _references;

		public ForeignKeyShim(string name, string referencedTable, string referencedSchema, IEnumerable<IColumnReference> references) : this(null, name, referencedTable, referencedSchema, references)
		{

		}

		public ForeignKeyShim(IForeignKey @base, string name = null, string referencedTable = null, string referencedSchema = null, IEnumerable<IColumnReference> references = null)
		{
			_base=@base;
			_name=name;
			_referencedTable=referencedTable;
			_referencedSchema=referencedSchema;
			_references=references;
		}

		public string Name => _name ?? _base?.Name;

		public string ReferencedTable => _referencedTable ?? _base?.ReferencedTable;

		public string ReferencedSchema => _referencedSchema ?? _base.ReferencedSchema;

		public IEnumerable<IColumnReference> References => ShimHelper.NonNullConcat(_references, _base?.References).DistinctBy(x => new { x.SourceColumn, x.TargetColumn } );
	}
}
