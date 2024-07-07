using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.DataSources.Siard.Common;
using Relational2Rdf.DataSources.Siard.SqlMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Relational2Rdf.DataSources.Siard.SiardV1
{
	public partial class SchemaTypeV1 : ISiardSchema
	{
		public IEnumerable<IType> Types => Array.Empty<IType>();

		[XmlIgnore]
		IEnumerable<ITable> ISchema.Tables => this.tables;
	}

	public partial class TableTypeV1 : ISiardTable
	{
		[XmlIgnore]
		public IEnumerable<string> ColumnNames => this.columns.Select(x => x.Name);

		[XmlIgnore]
		public IEnumerable<IColumn> KeyColumns => PrimaryKey?.Column == null ? Array.Empty<IColumn>() : this.columns.Cast<IColumn>().Where(x => this.PrimaryKey.Column.Contains(x.Name));

		public int RowCount => int.Parse(this.Rows);

		IEnumerable<IForeignKey> ITable.ForeignKeys => this.foreignKeys;

		IEnumerable<IColumn> ITable.Columns => this.columns.Cast<IColumn>();

	}

	public partial class ColumnTypeV1 : IColumn
	{
		public string SourceType => Type;
		public string OriginalSourceType => TypeOriginal;
		public string UdtType => null;
		public string UdtSchema => null;

		public bool IsUdt => false;
		public bool IsArray => false;

		public CommonType CommonType => SqlTypeMapping.GetFromSql(Type);

		public IEnumerable<IField> Fields => Array.Empty<IField>();

		public AttributeType AttributeType => AttributeType.Value;

		public int? Cardinality => 1;
	}

	public partial class ForeignKeyTypeV1 : IForeignKey
	{
		public IEnumerable<IColumnReference> References => this.References;
	}

	public partial class ReferenceTypeV1 : IColumnReference
	{
		public string SourceColumn => this.Column;

		public string TargetColumn => this.Referenced;
	}
}
