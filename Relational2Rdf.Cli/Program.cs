using Cocona;
using Microsoft.Extensions.Logging;
using Relational2Rdf.Cli;
using Relational2Rdf.Converter.Utils;
using Relational2Rdf.DataSources.Siard;
using System.Text.Json;

var builder = CoconaApp.CreateBuilder(args);
var app = builder.Build();

app.AddCommand("siard", RunSiard);
await app.RunAsync();

static async Task RunSiard([Argument(Name = "Siard File", Description = "Path to the archive file to convert")] string siardFile, ConversionParameters parameters)
{
	var config = parameters.BuildConfig();
	var reader = new SiardFileReader();
	var dataSource = await reader.ReadAsync(siardFile);
	var outputFile = await Relational2Rdf.Converter.Converter.ConvertAsync(dataSource, parameters.OutputDirectory, config);
	if (parameters.Trace)
	{
		var traceName = outputFile + ".trace.json";
		Dictionary<string, IEnumerable<ProfilerRecord>> records = Profiler.GetCategories().ToDictionary(x => x, x => Profiler.GetRecords(x));
		using var trace = File.Create(traceName);
		await JsonSerializer.SerializeAsync(trace, records);
		await trace.FlushAsync();
	}
}