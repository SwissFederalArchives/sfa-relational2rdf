using AwosFramework.Rdf.Lib;
using Relational2Rdf.Converter.Conversion;
using Relational2Rdf.Converter.Conversion.Settings;
using Relational2Rdf.DataSources.Siard;
using Relational2Rdf.DataSources.Siard.Common;
using System.ComponentModel.DataAnnotations;
using System.Text;

var reader = new SiardFileReader();
using var source = await reader.ReadAsync("./Resources/ech-0165_oe.siard");



Console.WriteLine(source.Name);

var writer = WriterFactory.TurtleWriter($"{source.Name}.ttl");
//var schema = source.Schemas.First();
//var table = schema.Tables.First();

var settings = new ConversionSettings()
{
	//AiEndpoint = "https://api.mistral.ai",
	//AiModel = "mistral-medium"
};


var context = new ConversionContext(settings, source, writer);
await context.InitAsync();
foreach (var schema in source.Schemas)
{
	Console.WriteLine($"Converting Schema {schema.Name}");
	foreach (var table in schema.Tables)
	{
		var top = Console.CursorTop;
		var progressAction = new Action<long>(x =>
		{
			Console.CursorTop = top;
			Console.CursorLeft = 0;
			Console.Write($"Converting {table.Name} {x}/{table.RowCount}");
		});

		var converter = new TableConverter(context, writer, source.GetReader(schema, table), settings.TableSettings);
		await converter.ConvertAsync(progressAction);
		Console.CursorTop = top;
		Console.CursorLeft = 0;
		Console.WriteLine($"Converted Table {table.Name}".PadRight(Console.WindowWidth - 1, ' '));
	}
}
writer.Dispose();

//var builder = new StringBuilder();

//builder.AppendLine("{");
//void appendIfNotFirst(ref bool first, string value)
//{
//	if (first)
//		first = false;
//	else
//		builder.AppendLine(value);
//}

//bool firstSchema = true;
//foreach (var schema in source.Schemas)
//{
//	appendIfNotFirst(ref firstSchema, ",");
//	Console.WriteLine($"processing schema {schema.Name}");
//	builder.Append($"  \"{schema.Name}\": {{");
//	bool firstTable = true;
//	foreach (var table in schema.Tables)
//	{
//		Console.WriteLine($"processing table {table.Name}");
//		appendIfNotFirst(ref firstTable, ",");
//		builder.Append($"    \"{table.Name}\": [");
//		using var treader = source.GetReader(schema, table);
//		bool firstRow = true;
//		while (treader.ReadNext(out var row))
//		{
//			appendIfNotFirst(ref firstRow, ",");
//			if (row is RowImplementation rowImpl)
//			{
//				builder.Append($"      {rowImpl.ToJson()}");
//			}
//		}

//		Console.WriteLine($"read {table.RowCount} rows");
//		builder.Append("    ]");
//	}
//	builder.AppendLine("  }");
//}

//builder.AppendLine("}");
//File.WriteAllText($"{source.Name}.json", builder.ToString());