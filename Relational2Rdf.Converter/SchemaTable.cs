using Relational2Rdf.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter
{
	public record SchemaTable(ISchema Schema, ITable Table)
	{
		public override string ToString()
		{
			return $"{nameof(SchemaTable)} {{ Schema = {Schema.Name}, Table = {Table.Name} }}";
		}
	}
}
