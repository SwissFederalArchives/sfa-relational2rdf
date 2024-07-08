using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Ontology;
using Relational2Rdf.Converter.Ontology.Conversion.ReferenceMeta;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology.Conversion.ConversionMeta
{
	public static class MetaBuilder
	{
		public static IConversionMeta BuildConversionMeta(OntologyConversionContext ctx, ISchema schema, IType type)
		{
			var iri = ctx.GetTypeIri(schema, type);
			var attributes = ctx.DataSource.GetAllAttributes(type);
			var valueAttributes = attributes;
			var attributeMap = attributes.ToFrozenDictionary(x => x.Name);
			var udtAttributes = attributes.Where(x => x.AttributeType == AttributeType.Udt || x.AttributeType == AttributeType.UdtArray);
			var nestedMetas = new Dictionary<IAttribute, IConversionMeta>();
			int counter = 1;
			var attrItemInfos = type.Attributes.Select(x => new AttributeItemInfo(x, ctx.GetAttributeIri(schema, type, x), $"c{counter++}")).ToFrozenDictionary(x => x.Attribute);


			foreach (var attr in udtAttributes)
			{
				var udtType = ctx.DataSource.FindType(attr.UdtSchema, attr.UdtType);
				var udtSchema = ctx.DataSource.FindSchema(attr.UdtSchema);
				var udtMeta = BuildConversionMeta(ctx, udtSchema, udtType);
				nestedMetas[attr] = udtMeta;
			}

			return new NoKeyConversionMeta
			{
				TypeIri = iri,
				NestedMetas = nestedMetas.ToFrozenDictionary(),
				References = Array.Empty<IReferenceMeta>(),
				TypeName = type.Name,
				AttributeItemInfos = attrItemInfos,
				Attributes = type.Attributes.ToArray(),
				Counter = ctx.GetCounter(type.Name),
				RowBaseIri = iri.Extend("row")
			};
		}

		public static IReferenceMeta[] BuildReferenceMetas(OntologyConversionContext ctx, ISchema schema, ITable table)
		{
			var fKeys = table.ForeignKeys.ToArray();
			IReferenceMeta[] metas = new IReferenceMeta[fKeys.Length];

			for (int i = 0; i < fKeys.Length; i++)
			{
				var key = fKeys[i];
				var targetRowIri = ctx.GetTypeIri(key.ReferencedSchema, key.ReferencedTable).Extend("row");
				var attrs = key.References.Select(x => table.Columns.First(y => y.Name == x.SourceColumn)).ToArray();
				var fkIri = ctx.GetForeignKeyIri(schema, table, key);

				if (key.References.Count() == 1)
				{
					metas[i] = new SingleKeyReferenceMeta
					{
						ForeignKey = key,
						ForeignKeyColumn = key.References.First().SourceColumn,
						TargetRowIri = targetRowIri,
						SourceAttributes = attrs,
						ForeignKeyIri = fkIri
					};
				}
				else
				{
					metas[i] = new MultiKeyReferenceMeta
					{
						ForeignKey = key,
						TargetRowIri = targetRowIri,
						ForeignKeyColumns = key.References.Select(x => x.SourceColumn).ToArray(),
						SourceAttributes = attrs,
						ForeignKeyIri = fkIri
					};
				}
			}

			return metas;
		}

		public static IConversionMeta BuildConversionMeta(OntologyConversionContext ctx, ISchema schema, ITable table)
		{
			var iri = ctx.GetTableIri(schema, table);

			var refColumnNames = table.ForeignKeys.SelectMany(x => x.References).Select(x => x.SourceColumn).Distinct().ToArray();
			var valueColumns = table.Columns.Where(x => refColumnNames.Contains(x.Name) == false);
			
			int counter = 1;
			var attrItemInfos = table.Columns
				.Select(x => new AttributeItemInfo(x, ctx.GetColumnIri(schema, table, x), $"c{counter++}"))
				.ToFrozenDictionary(x => x.Attribute);
			
			var udtColumns = table.Columns.Where(x => x.AttributeType == AttributeType.Udt || x.AttributeType == AttributeType.UdtArray);
			var refColumns = table.Columns.Where(x => refColumnNames.Contains(x.Name));	
			var udtMetas = new Dictionary<IAttribute, IConversionMeta>();
			var nestedMetas = new Dictionary<IAttribute, IConversionMeta>();
			foreach (var attr in udtColumns)
			{
				var udtType = ctx.DataSource.FindType(attr.UdtSchema, attr.UdtType);
				var udtSchema = ctx.DataSource.FindSchema(attr.UdtSchema);
				var udtMeta = BuildConversionMeta(ctx, udtSchema, udtType);
				nestedMetas[attr] = udtMeta;
			}

			var references = BuildReferenceMetas(ctx, schema, table);
			var keyCount = table.KeyColumns.Count();
			if (keyCount == 1)
			{
				return new SingleKeyConversionMeta
				{
					TypeName = table.Name,
					TypeIri = iri,
					KeyColumn = table.KeyColumns.First().Name,
					References = references,
					Attributes = table.Columns.Cast<IAttribute>().ToArray(),
					NeedsEscaping = table.KeyColumns.First().CommonType != CommonType.Integer,
					NestedMetas = nestedMetas.ToFrozenDictionary(),
					AttributeItemInfos = attrItemInfos,
					RowBaseIri = iri.Extend("row")
				};
			}
			else if (keyCount > 1)
			{
				return new MultiKeyConversionMeta
				{
					TypeName = table.Name,
					TypeIri = iri,
					KeyColumns = table.KeyColumns.Select(x => x.Name).ToArray(),
					References = references,
					Attributes = valueColumns.Cast<IAttribute>().ToArray(),
					NestedMetas = nestedMetas.ToFrozenDictionary(),
					AttributeItemInfos = attrItemInfos,
					RowBaseIri = iri.Extend("row")
				};
			}
			else
			{
				return new NoKeyConversionMeta
				{
					TypeName = table.Name,
					TypeIri = iri,
					Counter = ctx.GetCounter(table.Name),
					References = references,
					Attributes = valueColumns.Cast<IAttribute>().ToArray(),
					AttributeItemInfos = attrItemInfos,
					NestedMetas = nestedMetas.ToFrozenDictionary(),
					RowBaseIri = iri.Extend("row")
				};
			}
		}
	}
}
