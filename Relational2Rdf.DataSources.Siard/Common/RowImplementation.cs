using Microsoft.Extensions.ObjectPool;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.DataSources.Siard.Utils;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Relational2Rdf.DataSources.Siard.Common
{
	internal class RowImplementation : IRow
	{
		private const int MAX_ARRAY_PRE_INSTANCE_SIZE = 64;
		public IAttribute[] Attributes { get; init; }
		public IAttributeMeta[] AttributeMetas { get; init; }

		public object[] NestedObjects { get; init; }
		public object[] Items { get; init; }

		private readonly IDictionary<string, int> _itemIndexMapping;
		private readonly IDictionary<string, IAttribute> _attributeKeyMapping;
		private readonly Dictionary<int, ObjectPool<RowImplementation>> _rowPools = new();

		public IAttribute GetAttribute(string key) => _attributeKeyMapping[key];
		public bool TryGetAttribute(string key, out IAttribute attr) => _attributeKeyMapping.TryGetValue(key, out attr);

		// default constructor so ObjectPool doesnt bitch around
		public RowImplementation()
		{
			throw new NotImplementedException();
		}

		public RowImplementation(IRelationalDataSource source, IEnumerable<IAttribute> attributes, IEnumerable<IAttributeMeta> fields)
		{
			Attributes = attributes.ToArray();
			Items = new object[Attributes.Length];
			NestedObjects = new object[Attributes.Length];
			AttributeMetas = fields.ToArray();

			var attrMap = Attributes.ToDictionary(x => x.Name, x => x);

			var mapping = new Dictionary<string, int>();
			for (int i = 0; i < Attributes.Length; i++)
			{
				var attr = Attributes[i];
				switch (attr.AttributeType)
				{
					case AttributeType.UdtArray:
						{
							var type = source.FindType(attr.UdtSchema, attr.UdtType);
							var listPool = new PooledList<RowImplementation>(attr.Cardinality ?? 0, new RowPoolPolicy(source.GetAllAttributes(type).ToArray(), source, AttributeMetas[i].Metas.ToArray()));
							NestedObjects[i] = listPool;
							break;
						}

					case AttributeType.Udt:
						{
							var type = source.FindType(attr.UdtSchema, attr.UdtType);
							NestedObjects[i] = new RowImplementation(source, source.GetAllAttributes(type), AttributeMetas[i].Metas);
							break;
						}

					case AttributeType.Array:
						{
							NestedObjects[i] = new List<object>(Math.Min(attr.Cardinality.HasValue ? attr.Cardinality.Value : 1, MAX_ARRAY_PRE_INSTANCE_SIZE));
							break;
						}
				}

				mapping[attr.Name] = i;

				// for some reason siard encodes columns with numeric rising order so column 1 => c1 same for udt fields udt field 1 => u1
				var readerKey = $"{(attr is IColumn ? "c" : "u")}{i+1}";
				mapping[readerKey] = i;
				attrMap[readerKey] = attr;

			}
			_itemIndexMapping = mapping.ToFrozenDictionary();
			_attributeKeyMapping = attrMap.ToFrozenDictionary();
		}

		public void Clear()
		{
			Array.Clear(Items);
		}

		public void Write(string name, object value) => Write(_itemIndexMapping[name], value);
		public void Write(int index, object value)
		{
			Items[index] = value;
		}

		public List<object> WriteValueArray(string name) => WriteValueArray(_itemIndexMapping[name]);
		public List<object> WriteValueArray(int index)
		{
			var list = NestedObjects[index] as List<object>;
			list.Clear();
			Items[index] = list;
			return list;
		}

		public RowImplementation WriteUdt(string name) => WriteUdt(_itemIndexMapping[name]);
		public RowImplementation WriteUdt(int index)
		{
			var row = NestedObjects[index] as RowImplementation;
			row.Clear();
			Items[index] = (IRow)row;
			return row;
		}

		public Func<RowImplementation> WriteUdtArray(string name) => WriteUdtArray(_itemIndexMapping[name]);
		public Func<RowImplementation> WriteUdtArray(int index)
		{
			var pool = NestedObjects[index] as PooledList<RowImplementation>;
			pool.Clear();
			Items[index] = pool.List.Cast<IRow>();
			return pool.GetNext;
		}

		public IAttributeMeta GetAttributeMeta(int index) => AttributeMetas[index];
		public IAttributeMeta GetAttributeMeta(string name) => AttributeMetas[_itemIndexMapping[name]];

		public object GetItem(int index) => Items[index];

		public object GetItem(string column)
		{
			return Items[_itemIndexMapping[column]];
		}

		public override string ToString() => ToJson();

		public string ToJson()
		{
			string readStream(Stream stream)
			{
				using var reader = new StreamReader(stream);
				var res = reader.ReadToEnd();
				return HttpUtility.JavaScriptStringEncode(res);
			}

			var content = string.Join(", ", Attributes.Select(x => {
				var item = Items[_itemIndexMapping[x.Name]];
				if (item == null)
					return null;

				string value = item switch
				{
					string @string => $"\"{HttpUtility.JavaScriptStringEncode(@string)}\"",
					IBlob stream => $"\"{readStream(stream.GetStream())}\"",
					IEnumerable<object> objects => $"[{string.Join(", ", objects.Select(x => $"\"{x}\""))}]",
					_ => item.ToString()
				};

				return $"\"{x.Name}\": {value}";
			}).Where(x => x != null));
			return $"{{{content}}}";
		}

		public IEnumerable<(IAttribute Attribute, object Value)> Enumerate()
		{
			for (int i = 0; i < Attributes.Length; i++)
				yield return (Attributes[i], Items[i]);
		}
	}
}
