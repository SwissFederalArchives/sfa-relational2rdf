using Relational2Rdf.Converter;
using Relational2Rdf.Converter.Conversion.Settings;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

var file = new FileInfo("./Resources/stapfer.siard");
var output = new DirectoryInfo("./out");
output.Create();
var settings = new ConversionSettings()
{
};

await Converter.ConvertAsync(file, output, settings);
Console.WriteLine("Press any key to show profiling results");
Console.ReadKey();
Console.Clear();
foreach(var category in Profiler.GetCategories())
{
	Console.WriteLine(category);
	Console.WriteLine(new string('=', Console.WindowWidth));
	foreach(var record in Profiler.GetRecords(category))
	{
		var line = $"{record.Unit.Pad(40)}|{record.Duration.TotalMilliseconds.ToString("0.##").PadLeft(8)}ms|{record.Message.Pad(Console.WindowWidth - 52)}";
		Console.WriteLine(line);
	}

	Console.WriteLine();
}