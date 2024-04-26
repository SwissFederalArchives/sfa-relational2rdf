using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Relational2Rdf.DataSources.Siard.Utils
{
	public static class Extensions
	{
		public static T AddAndReturn<T>(this IList<T> list, T item)
		{
			list.Add(item);
			return item;
		}

		private static readonly XmlNodeType[] INTERESTING_NODE_TYPES = new[] { XmlNodeType.Attribute, XmlNodeType.Element, XmlNodeType.Text, XmlNodeType.EndElement };
		public static bool ReadRelevant(this XmlReader reader, XmlNodeType[] nodeTypes = null)
		{
			nodeTypes ??= INTERESTING_NODE_TYPES;

			do
			{
				if (reader.Read() == false)
					return false;
			} while (INTERESTING_NODE_TYPES.Contains(reader.NodeType) == false);

			return true;
		}


		public static void CheckElement(this XmlReader reader, string errorMessage, string tag = null, string tagErrorMessage = null)
		{
			if (reader.NodeType != XmlNodeType.Element)
				throw new ArgumentException(errorMessage);

			if (tag != null && reader.Name != tag)
				throw new ArgumentException(tagErrorMessage);
		}

		public static void CheckEndElement(this XmlReader reader, string errorMessage, string tag = null, string tagErrorMessage = null)
		{
			if (reader.NodeType != XmlNodeType.EndElement)
				throw new ArgumentException(errorMessage);

			if (tag != null && reader.Name != tag)
				throw new ArgumentException(tagErrorMessage);
		}

		public static void CheckValue(this XmlReader reader, string errorMessage)
		{
			if (reader.NodeType != XmlNodeType.Text)
				throw new ArgumentException(errorMessage);
		}
	}
}
