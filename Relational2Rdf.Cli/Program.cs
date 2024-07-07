using AwosFramework.Factories;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Relational2Rdf.Cli;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter;
using Relational2Rdf.Converter.Ai;
using Relational2Rdf.Converter.Utils;
using Relational2Rdf.DataSources.Siard;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;

var builder = CoconaApp.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddDebug();
var app = builder.Build();


app.AddCommand("siard", () => Console.WriteLine("specify one output provider")).OptionLikeCommand(x =>
{
	x.Add("ai", RunSiardAi);
});

await app.RunAsync();


async Task HandleTraceAsync(ConversionParameters settings, string outputFile)
{
	if (settings.Trace)
	{
		var traceName = outputFile + ".trace.json";
		Dictionary<string, IEnumerable<ProfilerRecord>> records = Profiler.GetCategories().ToDictionary(x => x, x => Profiler.GetRecords(x));
		using var trace = File.Create(traceName);
		await JsonSerializer.SerializeAsync(trace, records);
		await trace.FlushAsync();
	}

	Profiler.Clear();
}

async Task RunSiardAi([Argument(Name = "Siard File", Description = "Path to the archive file to convert")] string siardFile, ConversionParameters parameters)
{
	var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
	var reader = new SiardFileReader();
	var factory = new AiConveterFactory(parameters.BuildAiConfig());
	var converter = new ConversionsManager(parameters.BuildConverterConfig(), factory, loggerFactory);
	var attr = File.GetAttributes(siardFile);
	var files = attr.HasFlag(FileAttributes.Directory) ? Directory.GetFiles(siardFile, "*.siard") : new string[] { siardFile };
	var logger = loggerFactory.CreateLogger("Relational2Rdf");
	foreach (var file in files)
	{
		logger.LogInformation("Converting {0}", file);
		var dataSource = await reader.ReadAsync(file);
		var outputFile = await converter.ConvertAsync(dataSource);
		await HandleTraceAsync(parameters, outputFile);
		logger.LogInformation("Conversion complete. Output written to {0}", outputFile);
	}
}