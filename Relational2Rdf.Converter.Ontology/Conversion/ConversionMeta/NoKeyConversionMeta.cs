﻿using AwosFramework.Rdf.Lib.Core;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Ontology.Conversion.ReferenceMeta;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ontology.Conversion.ConversionMeta
{
	public class NoKeyConversionMeta : IConversionMeta
	{
		public string TypeName { get; init; }
		public IRI TypeIri { get; init; }
		public IAttribute[] Attributes { get; init; }
		public IDictionary<IAttribute, IConversionMeta> NestedMetas { get; init; }
		public IDictionary<IAttribute, IRI> PredicateNames { get; init; }
		public AtomicCounter Counter { get; init; }
		public FrozenDictionary<IAttribute, AttributeItemInfo> AttributeItemInfos { get; init; }
		public IRI RowBaseIri { get; init; }

		public IReferenceMeta[] References { get; init; }

		public string GetKey(IRow row) => Counter.GetNext().ToString(); 
		public IConversionMeta GetNestedMeta(IAttribute attr) => NestedMetas[attr];
		public IRI GetPredicate(IAttribute attr) => PredicateNames[attr];
	}
}
