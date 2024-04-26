using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relation2Rdf.Common.Shims
{
	public class ColumnReferenceShim : IColumnReference
	{
		private IColumnReference _base;
		private string _sourceColumn;
		private string _targetColumn;

		public ColumnReferenceShim(string sourceColumn, string targetColumn) : this(null, sourceColumn, targetColumn)
		{

		}

		public ColumnReferenceShim(IColumnReference @base, string sourceColumn = null, string targetColumn = null)
		{
			_base=@base;
			_sourceColumn=sourceColumn;
			_targetColumn=targetColumn;
		}

		public string SourceColumn => _sourceColumn ?? _base?.SourceColumn;
		public string TargetColumn => _targetColumn ?? _base?.TargetColumn;
	}
}
