// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code++. Version 6.0.75.0. www.xsd2code.com
//    {"NameSpace":"Relational2Rdf.DataSources.Siard","Properties":{"AutomaticProperties":true,"PascalCaseProperty":true},"XmlAttribute":{"Enabled":true},"ClassParams":{},"Miscellaneous":{}}
//  </auto-generated>
// ------------------------------------------------------------------------------
#pragma warning disable
namespace Relational2Rdf.DataSources.Siard.SiardV2
{
	using System;
	using System.Diagnostics;
	using System.Xml.Serialization;
	using System.Runtime.Serialization;
	using System.Collections;
	using System.Xml.Schema;
	using System.ComponentModel;
	using System.Xml;
	using System.Collections.Generic;



	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "siardArchive")]
	[XmlRootAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", IsNullable = false, ElementName = "siardArchive")]
	public partial class SiardArchiveV2
	{
		[XmlElement("dbname")]
		public string Dbname { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
		[XmlElement("archiver")]
		public string Archiver { get; set; }
		[XmlElement("archiverContact")]
		public string ArchiverContact { get; set; }
		[XmlElement("dataOwner")]
		public string DataOwner { get; set; }
		[XmlElement("dataOriginTimespan")]
		public string DataOriginTimespan { get; set; }
		[XmlElement(DataType = "anyURI", ElementName = "lobFolder")]
		public string LobFolder { get; set; }
		[XmlElement("producerApplication")]
		public string ProducerApplication { get; set; }
		[XmlElement(DataType = "date", ElementName = "archivalDate")]
		public System.DateTime ArchivalDate { get; set; }
		[XmlElement("messageDigest", ElementName = "messageDigest")]
		public List<MessageDigestTypeV2> MessageDigest { get; set; }
		[XmlElement("clientMachine")]
		public string ClientMachine { get; set; }
		[XmlElement("databaseProduct")]
		public string DatabaseProduct { get; set; }
		[XmlElement("connection")]
		public string Connection { get; set; }
		[XmlElement("databaseUser")]
		public string DatabaseUser { get; set; }
		[XmlArrayItemAttribute("schema", IsNullable = false)]
		public List<SchemaTypeV2> schemas { get; set; }
		[XmlArrayItemAttribute("user", IsNullable = false)]
		public List<UserTypeV2> users { get; set; }
		[XmlArrayItemAttribute("role", IsNullable = false)]
		public List<RoleTypeV2> roles { get; set; }
		[XmlArrayItemAttribute("privilege", IsNullable = false)]
		public List<PrivilegeTypeV2> privileges { get; set; }
		[XmlAttribute(AttributeName = "version")]
		public VersionTypeV2 Version { get; set; }

		public SiardArchiveV2()
		{
			privileges = new List<PrivilegeTypeV2>();
			roles = new List<RoleTypeV2>();
			users = new List<UserTypeV2>();
			schemas = new List<SchemaTypeV2>();
			MessageDigest = new List<MessageDigestTypeV2>();
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "messageDigestType")]
	[XmlRootAttribute("messageDigestType")]
	public partial class MessageDigestTypeV2
	{
		[XmlElement("digestType")]
		public DigestTypeTypeV2 DigestType { get; set; }
		[XmlElement("digest")]
		public string Digest { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "digestTypeType")]
	[XmlRootAttribute("digestTypeType")]
	public enum DigestTypeTypeV2
	{
		MD5,
		[XmlEnumAttribute("SHA-1")]
		SHA1,
		[XmlEnumAttribute("SHA-256")]
		SHA256,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "privilegeType")]
	[XmlRootAttribute("privilegeType")]
	public partial class PrivilegeTypeV2
	{
		[XmlElement("type")]
		public string Type { get; set; }
		[XmlElement("object")]
		public string Object { get; set; }
		[XmlElement("grantor")]
		public string Grantor { get; set; }
		[XmlElement("grantee")]
		public string Grantee { get; set; }
		[XmlElement("option")]
		public PrivOptionTypeV2 Option { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "privOptionType")]
	[XmlRootAttribute("privOptionType")]
	public enum PrivOptionTypeV2
	{
		ADMIN,
		GRANT,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "roleType")]
	[XmlRootAttribute("roleType")]
	public partial class RoleTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("admin")]
		public string Admin { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "userType")]
	[XmlRootAttribute("userType")]
	public partial class UserTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "parameterType")]
	[XmlRootAttribute("parameterType")]
	public partial class ParameterTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("mode")]
		public string Mode { get; set; }
		[XmlElement("type")]
		[XmlElement("typeName")]
		[XmlElement("typeSchema")]
		[XmlChoiceIdentifierAttribute("ItemsElementName")]
		public string[] Items { get; set; }
		[XmlElement("ItemsElementName")]
		[XmlIgnore]
		public ItemsChoiceType2V2[] ItemsElementName { get; set; }
		[XmlElement("typeOriginal")]
		public string TypeOriginal { get; set; }
		[XmlElement(DataType = "integer", ElementName = "cardinality")]
		public string Cardinality { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", IncludeInSchema = false)]
	[XmlRootAttribute("ItemsChoiceType2")]
	public enum ItemsChoiceType2V2
	{
		type,
		typeName,
		typeSchema,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "routineType")]
	[XmlRootAttribute("routineType")]
	public partial class RoutineTypeV2
	{
		[XmlElement("specificName")]
		public string SpecificName { get; set; }
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
		[XmlElement("source")]
		public string Source { get; set; }
		[XmlElement("body")]
		public string Body { get; set; }
		[XmlElement("characteristic")]
		public string Characteristic { get; set; }
		[XmlElement("returnType")]
		public string ReturnType { get; set; }
		[XmlArrayItemAttribute("parameter", IsNullable = false)]
		public List<ParameterTypeV2> parameters { get; set; }

		public RoutineTypeV2()
		{
			parameters = new List<ParameterTypeV2>();
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "viewType")]
	[XmlRootAttribute("viewType")]
	public partial class ViewTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("query")]
		public string Query { get; set; }
		[XmlElement("queryOriginal")]
		public string QueryOriginal { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
		[XmlArrayItemAttribute("column", IsNullable = false)]
		public List<ColumnTypeV2> columns { get; set; }
		[XmlElement(DataType = "integer", ElementName = "rows")]
		public string Rows { get; set; }

		public ViewTypeV2()
		{
			columns = new List<ColumnTypeV2>();
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "columnType")]
	[XmlRootAttribute("columnType")]
	public partial class ColumnTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement(DataType = "anyURI", ElementName = "lobFolder")]
		public string LobFolder { get; set; }
		[XmlElement("mimeType")]
		[XmlElement("type")]
		[XmlElement("typeName")]
		[XmlElement("typeSchema")]
		[XmlChoiceIdentifierAttribute("ItemsElementName")]
		public string[] Items { get; set; }
		[XmlElement("ItemsElementName")]
		[XmlIgnore]
		public ItemsChoiceType1V2[] ItemsElementName { get; set; }
		[XmlElement("typeOriginal")]
		public string TypeOriginal { get; set; }
		[XmlArrayItemAttribute("field", IsNullable = false)]
		public List<FieldTypeV2> fields { get; set; }
		[XmlElement("nullable")]
		public bool Nullable { get; set; }
		[XmlElement("defaultValue")]
		public string DefaultValue { get; set; }
		[XmlElement(DataType = "integer", ElementName = "cardinality")]
		public string Cardinality { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }

		public ColumnTypeV2()
		{
			fields = new List<FieldTypeV2>();
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", IncludeInSchema = false)]
	[XmlRootAttribute("ItemsChoiceType1")]
	public enum ItemsChoiceType1V2
	{
		mimeType,
		type,
		typeName,
		typeSchema,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "fieldType")]
	[XmlRootAttribute("fieldType")]
	public partial class FieldTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement(DataType = "anyURI", ElementName = "lobFolder")]
		public string LobFolder { get; set; }
		[XmlArrayItemAttribute("field", IsNullable = false)]
		public List<FieldTypeV2> fields { get; set; }
		[XmlElement("mimeType")]
		public string MimeType { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }

		public FieldTypeV2()
		{
			fields = new List<FieldTypeV2>();
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "triggerType")]
	[XmlRootAttribute("triggerType")]
	public partial class TriggerTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("actionTime")]
		public ActionTimeTypeV2 ActionTime { get; set; }
		[XmlElement("triggerEvent")]
		public string TriggerEvent { get; set; }
		[XmlElement("aliasList")]
		public string AliasList { get; set; }
		[XmlElement("triggeredAction")]
		public string TriggeredAction { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "actionTimeType")]
	[XmlRootAttribute("actionTimeType")]
	public enum ActionTimeTypeV2
	{
		BEFORE,
		[XmlEnumAttribute("INSTEAD OF")]
		INSTEADOF,
		AFTER,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "checkConstraintType")]
	[XmlRootAttribute("checkConstraintType")]
	public partial class CheckConstraintTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("condition")]
		public string Condition { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "referenceType")]
	[XmlRootAttribute("referenceType")]
	public partial class ReferenceTypeV2
	{
		[XmlElement("column")]
		public string Column { get; set; }
		[XmlElement("referenced")]
		public string Referenced { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "foreignKeyType")]
	[XmlRootAttribute("foreignKeyType")]
	public partial class ForeignKeyTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("referencedSchema")]
		public string ReferencedSchema { get; set; }
		[XmlElement("referencedTable")]
		public string ReferencedTable { get; set; }
		[XmlElement("reference", ElementName = "reference")]
		public List<ReferenceTypeV2> Reference { get; set; }
		[XmlElement("matchType")]
		public MatchTypeTypeV2 MatchType { get; set; }
		[XmlElement("deleteAction")]
		public ReferentialActionTypeV2 DeleteAction { get; set; }
		[XmlElement("updateAction")]
		public ReferentialActionTypeV2 UpdateAction { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }

		public ForeignKeyTypeV2()
		{
			Reference = new List<ReferenceTypeV2>();
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "matchTypeType")]
	[XmlRootAttribute("matchTypeType")]
	public enum MatchTypeTypeV2
	{
		FULL,
		PARTIAL,
		SIMPLE,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "referentialActionType")]
	[XmlRootAttribute("referentialActionType")]
	public enum ReferentialActionTypeV2
	{
		CASCADE,
		[XmlEnumAttribute("SET NULL")]
		SETNULL,
		[XmlEnumAttribute("SET DEFAULT")]
		SETDEFAULT,
		RESTRICT,
		[XmlEnumAttribute("NO ACTION")]
		NOACTION,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "uniqueKeyType")]
	[XmlRootAttribute("uniqueKeyType")]
	public partial class UniqueKeyTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
		[XmlElement("column", ElementName = "column")]
		public List<string> Column { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "tableType")]
	[XmlRootAttribute("tableType")]
	public partial class TableTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("folder")]
		public string Folder { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
		[XmlArrayItemAttribute("column", IsNullable = false)]
		public List<ColumnTypeV2> columns { get; set; }
		[XmlElement("primaryKey")]
		public UniqueKeyTypeV2 PrimaryKey { get; set; }
		[XmlArrayItemAttribute("foreignKey", IsNullable = false)]
		public List<ForeignKeyTypeV2> foreignKeys { get; set; }
		[XmlArrayItemAttribute("candidateKey", IsNullable = false)]
		public List<UniqueKeyTypeV2> candidateKeys { get; set; }
		[XmlArrayItemAttribute("checkConstraint", IsNullable = false)]
		public List<CheckConstraintTypeV2> checkConstraints { get; set; }
		[XmlArrayItemAttribute("trigger", IsNullable = false)]
		public List<TriggerTypeV2> triggers { get; set; }
		[XmlElement(DataType = "integer", ElementName = "rows")]
		public string Rows { get; set; }

		public TableTypeV2()
		{
			triggers = new List<TriggerTypeV2>();
			checkConstraints = new List<CheckConstraintTypeV2>();
			candidateKeys = new List<UniqueKeyTypeV2>();
			foreignKeys = new List<ForeignKeyTypeV2>();
			PrimaryKey = new UniqueKeyTypeV2();
			columns = new List<ColumnTypeV2>();
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "attributeType")]
	[XmlRootAttribute("attributeType")]
	public partial class AttributeTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("type")]
		[XmlElement("typeName")]
		[XmlElement("typeSchema")]
		[XmlChoiceIdentifierAttribute("ItemsElementName")]
		public string[] Items { get; set; }
		[XmlElement("ItemsElementName")]
		[XmlIgnore]
		public ItemsChoiceTypeV2[] ItemsElementName { get; set; }
		[XmlElement("typeOriginal")]
		public string TypeOriginal { get; set; }
		[XmlElement("nullable")]
		public bool Nullable { get; set; }
		[XmlElement("defaultValue")]
		public string DefaultValue { get; set; }
		[XmlElement(DataType = "integer", ElementName = "cardinality")]
		public string Cardinality { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", IncludeInSchema = false)]
	[XmlRootAttribute("ItemsChoiceType")]
	public enum ItemsChoiceTypeV2
	{
		type,
		typeName,
		typeSchema,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "typeType")]
	[XmlRootAttribute("typeType")]
	public partial class TypeTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("category")]
		public CategoryTypeV2 Category { get; set; }
		[XmlElement("underSchema")]
		public string UnderSchema { get; set; }
		[XmlElement("underType")]
		public string UnderType { get; set; }
		[XmlElement("instantiable")]
		public bool Instantiable { get; set; }
		[XmlElement("final")]
		public bool Final { get; set; }
		[XmlElement("base")]
		public string Base { get; set; }
		[XmlArrayItemAttribute("attribute", IsNullable = false)]
		public List<AttributeTypeV2> attributes { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }

		public TypeTypeV2()
		{
			attributes = new List<AttributeTypeV2>();
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "categoryType")]
	[XmlRootAttribute("categoryType")]
	public enum CategoryTypeV2
	{
		distinct,
		udt,
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[DebuggerStepThrough]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "schemaType")]
	[XmlRootAttribute("schemaType")]
	public partial class SchemaTypeV2
	{
		[XmlElement("name")]
		public string Name { get; set; }
		[XmlElement("folder")]
		public string Folder { get; set; }
		[XmlElement("description")]
		public string Description { get; set; }
		[XmlArrayItemAttribute("type", IsNullable = false)]
		public List<TypeTypeV2> types { get; set; }
		[XmlArrayItemAttribute("table", IsNullable = false)]
		public List<TableTypeV2> tables { get; set; }
		[XmlArrayItemAttribute("view", IsNullable = false)]
		public List<ViewTypeV2> views { get; set; }
		[XmlArrayItemAttribute("routine", IsNullable = false)]
		public List<RoutineTypeV2> routines { get; set; }

		public SchemaTypeV2()
		{
			routines = new List<RoutineTypeV2>();
			views = new List<ViewTypeV2>();
			tables = new List<TableTypeV2>();
			types = new List<TypeTypeV2>();
		}
	}

	[System.CodeDom.Compiler.GeneratedCodeAttribute("Xsd2Code", "6.2.6")]
	[Serializable]
	[XmlTypeAttribute(Namespace = "http://www.bar.admin.ch/xmlns/siard/2/metadata.xsd", TypeName = "versionType")]
	[XmlRootAttribute("versionType")]
	public enum VersionTypeV2
	{
		[XmlEnumAttribute("2.2")]
		Siard2_2,
		[XmlEnumAttribute("2.1")]
		Siard2_1,
	}
}
#pragma warning restore