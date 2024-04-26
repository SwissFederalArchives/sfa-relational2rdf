using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.DataSources.Siard.Common;
using Relational2Rdf.DataSources.Siard.SqlMapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.SiardV2
{
	[SiardVersion(SiardVersion.Siard2)]
	public partial class SiardArchiveV2 : ISiardArchive
	{
		public IEnumerable<ISchema> Schemas => this.schemas;

		public string Name => this.Dbname;
		SiardVersion ISiardArchive.Version => SiardVersion.Siard2;
	}

	public partial class FieldTypeV2 : IAttributeMeta
	{
		IEnumerable<IAttributeMeta> IAttributeMeta.Metas => this.fields;
	}

	public partial class ColumnTypeV2 : AbstractHasDataSourceRef, IAttributeMeta
	{
		public string MimeType => GetMimeType();
		IEnumerable<IAttributeMeta> IAttributeMeta.Metas => this.fields;

		public string GetMimeType() => getChoiceValue(ItemsChoiceType1V2.mimeType);

		private bool IsUdt() => this.ItemsElementName.Contains(ItemsChoiceType1V2.typeName);
		private bool IsArray() => ((IColumn)this).Cardinality > 1;


		private CommonType getCommonType()
		{
			if (IsUdt())
			{
				var type = __internal_dataSource.FindType(UdtSchema, UdtType);
				if (type.Type == TypeType.Distinct)
					return type.BaseType;
				else
					return CommonType.Unknown;
			}
			else
			{
				return SqlTypeMapping.GetFromSql(this.SourceType);
			}
		}


		private AttributeType getAttrType()
		{
			var udt = IsUdt();
			var array = IsArray();

			if (udt)
			{
				var type = __internal_dataSource.FindType(UdtSchema, UdtType);
				if (type.Type == TypeType.Distinct)
					return array ? AttributeType.Array : AttributeType.Value;
				else
					return array ? AttributeType.UdtArray : AttributeType.Udt;
			}
			else if (array)
			{
				return AttributeType.Array;
			}
			else
			{
				return AttributeType.Value;
			}
		}
	}

	public partial class AttributeTypeV2 : AbstractHasDataSourceRef
	{
		private string getChoiceValue(ItemsChoiceTypeV2 choice)
		{
			var index = Array.IndexOf(this.ItemsElementName, choice);
			if (index == -1)
				return null;

			return this.Items[index];
		}

		private bool IsUdt() => this.ItemsElementName.Contains(ItemsChoiceTypeV2.typeName);
		private bool IsArray() => ((IAttribute)this).Cardinality > 1;


		private CommonType getCommonType()
		{
			if (IsUdt())
			{
				var type = __internal_dataSource.FindType(UdtSchema, UdtType);
				if (type.Type == TypeType.Distinct)
					return type.BaseType;
				else
					return CommonType.Unknown;
			}
			else
			{
				return SqlTypeMapping.GetFromSql(this.SourceType);
			}
		}


		private AttributeType getAttrType()
		{
			var udt = IsUdt();
			var array = IsArray();

			if (udt)
			{
				var type = __internal_dataSource.FindType(UdtSchema, UdtType);
				if (type.Type == TypeType.Distinct)
					return array ? AttributeType.Array : AttributeType.Value;
				else
					return array ? AttributeType.UdtArray : AttributeType.Udt;
			}
			else if (array)
			{
				return AttributeType.Array;
			}
			else
			{
				return AttributeType.Value;
			}
		}
	}

	public partial class TypeTypeV2 : ISiardType
	{
		public string SuperTypeName => this.UnderType;

		public string SuperTypeSchema => this.UnderSchema;

	}
}
