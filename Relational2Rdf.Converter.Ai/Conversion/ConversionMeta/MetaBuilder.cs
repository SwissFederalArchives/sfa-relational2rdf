using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Ai.Conversion.ReferenceMeta;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai.Conversion.ConversionMeta
{
	public static class MetaBuilder
	{
		public static async Task<IConversionMeta> BuildConversionMetaAsync(ConversionContext rctx, SchemaContext ctx, IType type)
		{
			var schema = ctx.Schema;
			var iri = rctx.GetTableIri(schema, type);
			var predIri = rctx.GetTablePredicateIri(schema, type);
			var typeName = ctx.GetTableName(type);
			var attributes = rctx.DataSource.GetAllAttributes(type);
			var valueAttributes = attributes;
			var valuePredicateNames = await rctx.AiMagic.GetRdfRelationshipNamesAsync(typeName, valueAttributes.Select(x => x.Name));
			var attributeMap = attributes.ToFrozenDictionary(x => x.Name);
			var predicates = valuePredicateNames.ToFrozenDictionary(x => (IAttribute)attributeMap[x.Key], x => predIri.Extend(x.Value));
			var udtAttributes = attributes.Where(x => x.AttributeType == AttributeType.Udt || x.AttributeType == AttributeType.UdtArray);
			var nestedMetas = new Dictionary<IAttribute, IConversionMeta>();
			foreach (var attr in udtAttributes)
			{
				var udtType = rctx.DataSource.FindType(attr.UdtSchema, attr.UdtType);
				var udtSchemaCtx = rctx.GetSchemaContext(attr.UdtSchema);
				var udtMeta = await BuildConversionMetaAsync(rctx, udtSchemaCtx, udtType);
				nestedMetas[attr] = udtMeta;
			}

			return new NoKeyConversionMeta
			{
				BaseIri = iri,
				PredicateNames = predicates,
				SchemaIri = ctx.Iri,
				NestedMetas = nestedMetas.ToFrozenDictionary(),
				References = Array.Empty<IReferenceMeta>(),
				TypeName = typeName,
				ValueAttributes = valueAttributes.ToArray(),
				Counter = ctx.GetCounter(type)
			};
		}

		public static async Task<IManyToManyReferenceMeta> BuildManyToManyReferencesAsync(ConversionContext rctx, SchemaContext ctx, ITable table)
		{
			var fkeys = table.ForeignKeys.ToArray();
			var fkSource = fkeys[0];
			var fkTarget = fkeys[1];

			var sourceSchema = rctx.GetSchemaContext(fkSource.ReferencedSchema);
			var sourceTable = rctx.DataSource.FindTable(fkSource.ReferencedSchema, fkSource.ReferencedTable);
			var sourceColNames = sourceTable.Columns.ToArray();
			var sourceCols = fkTarget.References.OrderBy(x => Array.IndexOf(sourceColNames, x.TargetColumn)).Select(x => x.SourceColumn).ToArray();
			var sourceTableName = sourceSchema.GetTableName(sourceTable);
			var sourcePredIri = rctx.GetTablePredicateIri(sourceSchema.Schema, sourceTable);

			var targetSchema = rctx.GetSchemaContext(fkTarget.ReferencedSchema);
			var targetTable = rctx.DataSource.FindTable(fkSource.ReferencedSchema, fkSource.ReferencedTable);
			var targetColNames = targetTable.ColumnNames.ToArray();
			var targetCols = fkTarget.References.OrderBy(x => Array.IndexOf(targetColNames, x.TargetColumn)).Select(x => x.SourceColumn).ToArray();
			var targetTableName = targetSchema.GetTableName(targetTable);
			var targetPredIri = rctx.GetTablePredicateIri(targetSchema.Schema, targetTable);


			var predicates = await rctx.AiMagic.GetManyToManyNamesAsync(sourceTableName, targetTableName, ctx.GetTableName(table), fkSource.Name, fkTarget.Name);
			return new ManyToManyReferenceMeta
			{
				SourceTypeIri = rctx.GetTableIri(sourceSchema.Schema, sourceTable),
				TargetTypeIri = rctx.GetTableIri(targetSchema.Schema, targetTable),
				SourceToTargetPredicate = sourcePredIri.Extend(predicates.Forward),
				TargetToSourcePredicate = targetPredIri.Extend(predicates.Backward),
				SourceColumns = sourceCols.ToArray(),
				TargetColumns = targetCols.ToArray(),
			};
		}

		public static async Task<IReferenceMeta[]> BuildReferenceMetasAsync(ConversionContext rctx, SchemaContext ctx, ITable table)
		{
			var fKeys = table.ForeignKeys.ToArray();
			IReferenceMeta[] metas = new IReferenceMeta[fKeys.Length];
			var tableName = ctx.GetTableName(table);
			var refPredicateNames = await rctx.AiMagic.GetForeignKeyNamesAsync(tableName, table.ForeignKeys.Select(x => (x.Name, rctx.GetSchemaContext(x.ReferencedSchema).GetTableName(x.ReferencedTable))));
			for (int i = 0; i < fKeys.Length; i++)
			{
				var key = fKeys[i];
				var predIri = rctx.GetTablePredicateIri(ctx.Schema, table);
				var targetTableIri = rctx.GetTableIri(key.ReferencedSchema, key.ReferencedTable);
				var targetTablePredIri = rctx.GetTablePredicateIri(key.ReferencedSchema, key.ReferencedTable);

				var forward = predIri.Extend(refPredicateNames.Forward[key.Name]);
				var backward = targetTablePredIri.Extend(refPredicateNames.Backward[key.Name]);
				if (key.References.Count() == 1)
				{
					metas[i] = new SingleKeyReferenceMeta { BackwardPredicate = backward, ForwardPredicate = forward, ForeignKeyColumn = key.References.First().SourceColumn, TargeTypeIri = targetTableIri };
				}
				else
				{
					metas[i] = new MultiKeyReferenceMeta { BackwardPredicate = backward, ForwardPredicate = forward, TargeTypeIri = targetTableIri, ForeignKeyColumns = key.References.Select(x => x.SourceColumn).ToArray() };
				}
			}

			return metas;
		}

		public static async Task<IConversionMeta> BuildConversionMetaAsync(ConversionContext rctx, SchemaContext ctx, ITable table)
		{
			var schema = ctx.Schema;
			var iri = rctx.GetTableIri(schema, table);
			var predIri = rctx.GetTablePredicateIri(schema, table);

			var tableName = ctx.GetTableName(table.Name);
			var refColumnNames = table.ForeignKeys.SelectMany(x => x.References).Select(x => x.SourceColumn).Distinct().ToArray();
			var valueColumns = table.Columns.Where(x => refColumnNames.Contains(x.Name) == false);
			var udtColumns = table.Columns.Where(x => x.AttributeType == AttributeType.Udt || x.AttributeType == AttributeType.UdtArray);
			var refColumns = table.Columns.Where(x => refColumnNames.Contains(x.Name));
			var attributeMap = table.Columns.ToFrozenDictionary(x => x.Name);

			var valuePredicateNames = await rctx.AiMagic.GetRdfRelationshipNamesAsync(tableName, valueColumns.Select(x => x.Name));
			var predicates = valuePredicateNames.ToFrozenDictionary(x => (IAttribute)attributeMap[x.Key], x => predIri.Extend(x.Value));
			var udtMetas = new Dictionary<IAttribute, IConversionMeta>();
			var nestedMetas = new Dictionary<IAttribute, IConversionMeta>();
			foreach (var attr in udtColumns)
			{
				var udtType = rctx.DataSource.FindType(attr.UdtSchema, attr.UdtType);
				var udtSchemaCtx = rctx.GetSchemaContext(attr.UdtSchema);
				var udtMeta = await BuildConversionMetaAsync(rctx, udtSchemaCtx, udtType);
				nestedMetas[attr] = udtMeta;
			}

			var references = await BuildReferenceMetasAsync(rctx, ctx, table);
			var keyCount = table.KeyColumns.Count();
			if (keyCount == 1)
			{
				return new SingleKeyConversionMeta
				{
					TypeName = tableName,
					BaseIri = iri,
					KeyColumn = table.KeyColumns.First().Name,
					PredicateNames = predicates,
					References = references,
					ValueAttributes = valueColumns.Cast<IAttribute>().ToArray(),
					SchemaIri = ctx.Iri,
					NeedsEscaping = table.KeyColumns.First().CommonType != CommonType.Integer,
					NestedMetas = nestedMetas.ToFrozenDictionary(),
				};
			}
			else if (keyCount > 1)
			{
				return new MultiKeyConversionMeta
				{
					TypeName = tableName,
					BaseIri = iri,
					KeyColumns = table.KeyColumns.Select(x => x.Name).ToArray(),
					PredicateNames = predicates,
					References = references,
					ValueAttributes = valueColumns.Cast<IAttribute>().ToArray(),
					SchemaIri = ctx.Iri,
					NestedMetas = nestedMetas.ToFrozenDictionary()
				};
			}
			else
			{
				return new NoKeyConversionMeta
				{
					TypeName = tableName,
					BaseIri = iri,
					Counter = ctx.GetCounter(table),
					PredicateNames = predicates,
					References = references,
					ValueAttributes = valueColumns.Cast<IAttribute>().ToArray(),
					SchemaIri = ctx.Iri,
					NestedMetas = nestedMetas.ToFrozenDictionary()
				};
			}
		}
	}
}
