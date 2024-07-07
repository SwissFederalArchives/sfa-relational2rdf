using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.DataSources.Siard.Common;
using Relational2Rdf.DataSources.Siard.SqlMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Relational2Rdf.DataSources.Siard.SiardV2
{
	public partial class SchemaTypeV2 : ISiardSchema
	{
		public IEnumerable<IType> Types => this.types;

		[XmlIgnore]
		IEnumerable<ITable> ISchema.Tables => this.tables;
	}

	public partial class TableTypeV2 : ISiardTable
	{
		[XmlIgnore]
		public IEnumerable<string> ColumnNames => this.columns.Select(x => x.Name);

		[XmlIgnore]
		public IEnumerable<IColumn> KeyColumns => PrimaryKey?.Column == null ? Array.Empty<IColumn>() : this.columns.Where(x => this.PrimaryKey.Column.Contains(x.Name));

		public int RowCount => int.Parse(this.Rows);

		IEnumerable<IForeignKey> ITable.ForeignKeys => this.foreignKeys;

		IEnumerable<IColumn> ITable.Columns => this.columns;

	}

	public partial class TypeTypeV2 : IType
	{
		public bool HasSuperType => this.UnderType != null;

		public TypeType Type => this.Category == CategoryTypeV2.udt ? TypeType.UserDefined : TypeType.Distinct;

		public CommonType BaseType => Type == TypeType.Distinct ? SqlTypeMapping.GetFromSql(this.Base) : CommonType.Unknown;

		public IEnumerable<IAttribute> Attributes => this.attributes;
	}

	public partial class AttributeTypeV2 : IAttribute
	{
		public string SourceType => getChoiceValue(ItemsChoiceTypeV2.type);
		public string OriginalSourceType => TypeOriginal;	
		public string UdtType => getChoiceValue(ItemsChoiceTypeV2.typeName);
		public string UdtSchema => getChoiceValue(ItemsChoiceTypeV2.typeSchema);

		[XmlIgnore]
		private AttributeType? _attr;
		public AttributeType AttributeType
		{
			get
			{
				if (_attr == null)
					_attr = getAttrType();

				return _attr.Value;
			}
		}

		[XmlIgnore]
		private CommonType? _commonType;
		public CommonType CommonType
		{
			get
			{
				if (_commonType == null)
					_commonType = getCommonType();

				return _commonType.Value;
			}
		}


		int? IAttribute.Cardinality => this.Cardinality == null ? null : int.Parse(this.Cardinality);

	}

	public partial class ColumnTypeV2 : IColumn
	{
		private string getChoiceValue(ItemsChoiceType1V2 choice)
		{
			var index = Array.IndexOf(this.ItemsElementName, choice);
			if (index == -1)
				return null;

			return this.Items[index];
		}

		public string SourceType => getChoiceValue(ItemsChoiceType1V2.type);
		public string UdtType => getChoiceValue(ItemsChoiceType1V2.typeName);
		public string UdtSchema => getChoiceValue(ItemsChoiceType1V2.typeSchema);
		public string OriginalSourceType => TypeOriginal;

		[XmlIgnore]
		private AttributeType? _attr;
		public AttributeType AttributeType
		{
			get
			{
				if (_attr == null)
					_attr = getAttrType();

				return _attr.Value;
			}
		}

		[XmlIgnore]
		private CommonType? _commonType;
		public CommonType CommonType
		{
			get
			{
				if (_commonType == null)
					_commonType = getCommonType();

				return _commonType.Value;
			}
		}


		int? IAttribute.Cardinality => this.Cardinality == null ? null : int.Parse(this.Cardinality);
		public IEnumerable<IField> Fields => this.fields;
	}

	public partial class FieldTypeV2 : ISiardField
	{
		public IEnumerable<IField> Fields => this.fields;
		IEnumerable<ISiardField> ISiardField.SiardFields => this.fields;
	}

	public partial class ForeignKeyTypeV2 : IForeignKey
	{
		public IEnumerable<IColumnReference> References => this.Reference;
	}

	public partial class ReferenceTypeV2 : IColumnReference
	{
		public string SourceColumn => this.Column;

		public string TargetColumn => this.Referenced;
	}
}
