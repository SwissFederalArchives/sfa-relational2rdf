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
			var attrNames = type.Attributes.ToFrozenDictionary(x => x, x => $"c{counter++}");

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
				AttributeCellNames = attrNames,
				Attributes = type.Attributes.ToArray(),
				Counter = ctx.GetCounter(type.Name),
				RowBaseIri = iri.Extend("row")
			};
		}

		//public static async Task<IManyToManyReferenceMeta> BuildManyToManyReferencesAsync(ConversionContext rctx, SchemaContext ctx, ITable table)
		//{
		//	var fkeys = table.ForeignKeys.ToArray();
		//	var fkSource = fkeys[0];
		//	var fkTarget = fkeys[1];

		//	var sourceSchema = rctx.GetSchemaContext(fkSource.ReferencedSchema);
		//	var sourceTable = rctx.DataSource.FindTable(fkSource.ReferencedSchema, fkSource.ReferencedTable);
		//	var sourceColNames = sourceTable.Columns.ToArray();
		//	var sourceCols = fkTarget.References.OrderBy(x => Array.IndexOf(sourceColNames, x.TargetColumn)).Select(x => x.SourceColumn).ToArray();
		//	var sourceTableName = sourceSchema.GetTableName(sourceTable);
		//	var sourcePredIri = rctx.GetTablePredicateIri(sourceSchema.Schema, sourceTable);

		//	var targetSchema = rctx.GetSchemaContext(fkTarget.ReferencedSchema);
		//	var targetTable = rctx.DataSource.FindTable(fkSource.ReferencedSchema, fkSource.ReferencedTable);
		//	var targetColNames = targetTable.ColumnNames.ToArray();
		//	var targetCols = fkTarget.References.OrderBy(x => Array.IndexOf(targetColNames, x.TargetColumn)).Select(x => x.SourceColumn).ToArray();
		//	var targetTableName = targetSchema.GetTableName(targetTable);
		//	var targetPredIri = rctx.GetTablePredicateIri(targetSchema.Schema, targetTable);


		//	var predicates = await rctx.AiMagic.GetManyToManyNamesAsync(sourceTableName, targetTableName, ctx.GetTableName(table), fkSource.Name, fkTarget.Name);
		//	return new ManyToManyReferenceMeta
		//	{
		//		SourceTypeIri = rctx.GetTableIri(sourceSchema.Schema, sourceTable),
		//		TargetTypeIri = rctx.GetTableIri(targetSchema.Schema, targetTable),
		//		SourceToTargetPredicate = sourcePredIri.Extend(predicates.Forward),
		//		TargetToSourcePredicate = targetPredIri.Extend(predicates.Backward),
		//		SourceColumns = sourceCols.ToArray(),
		//		TargetColumns = targetCols.ToArray(),
		//	};
		//}

		public static IReferenceMeta[] BuildReferenceMetas(OntologyConversionContext ctx, ISchema schema, ITable table)
		{
			var fKeys = table.ForeignKeys.ToArray();
			IReferenceMeta[] metas = new IReferenceMeta[fKeys.Length];

			for (int i = 0; i < fKeys.Length; i++)
			{
				var key = fKeys[i];
				var targetTableIri = ctx.GetTypeIri(key.ReferencedSchema, key.ReferencedTable);
				var attrs = key.References.Select(x => table.Columns.First(y => y.Name == x.SourceColumn)).ToArray();
				var fkIri = ctx.GetForeignKeyIri(schema, table, key);

				if (key.References.Count() == 1)
				{
					metas[i] = new SingleKeyReferenceMeta
					{
						ForeignKey = key,
						ForeignKeyColumn = key.References.First().SourceColumn,
						TargeTypeIri = targetTableIri,
						SourceAttributes = attrs,
						ForeignKeyIri = fkIri
					};
				}
				else
				{
					metas[i] = new MultiKeyReferenceMeta
					{
						ForeignKey = key,
						TargeTypeIri = targetTableIri,
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
			var iri = ctx.GetTypeIri(schema, table);

			var refColumnNames = table.ForeignKeys.SelectMany(x => x.References).Select(x => x.SourceColumn).Distinct().ToArray();
			var valueColumns = table.Columns.Where(x => refColumnNames.Contains(x.Name) == false);
			int counter = 1;
			var attrNames = table.Columns.ToFrozenDictionary(x => (IAttribute)x, x => $"c{counter++}");
			var udtColumns = table.Columns.Where(x => x.AttributeType == AttributeType.Udt || x.AttributeType == AttributeType.UdtArray);
			var refColumns = table.Columns.Where(x => refColumnNames.Contains(x.Name));
			var attributeMap = table.Columns.ToFrozenDictionary(x => x.Name);

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
					AttributeCellNames = attrNames,
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
					AttributeCellNames = attrNames,
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
					AttributeCellNames = attrNames,
					NestedMetas = nestedMetas.ToFrozenDictionary(),
					RowBaseIri = iri.Extend("row")
				};
			}
		}
	}
}
