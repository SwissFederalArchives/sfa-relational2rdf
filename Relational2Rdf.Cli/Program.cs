using AwosFramework.Factories;
using Cocona;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NReco.Logging.File;
using Relational2Rdf.Cli;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter;
using Relational2Rdf.Converter.Ai;
using Relational2Rdf.Converter.Ontology;
using Relational2Rdf.Converter.Utils;
using Relational2Rdf.DataSources.Siard;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;

var builder = CoconaApp.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddDebug();
var app = builder.Build();


app.AddCommand("siard", RunSiard);
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

IConverterFactory GetConversionFactory(ConversionParameters parameters, ILoggerFactory factory)
{
	switch (parameters.ConverterType)
	{
		case ConverterType.Ai:
			return new AiConveterFactory(parameters.BuildAiConfig(), factory);

		case ConverterType.Ontology:
			return new OntologyTableConverterFactory(parameters.BuildOntologyConfig(), factory);

		default:
			throw new InvalidOperationException("Invalid converter type");
	}
}


async Task RunSiard([Argument(Name = "Siard File", Description = "Path to the archive file to convert")] string siardFile, ConversionParameters parameters)
{
	var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
	if(parameters.LogFile != null)
	{
		loggerFactory.AddFile(parameters.LogFile, opts =>
		{
			opts.MinLevel = parameters.LogLevel;
			opts.Append = true;
			opts.UseUtcTimestamp = true;
		});
	}

	var reader = new SiardFileReader();
	var factory = GetConversionFactory(parameters, loggerFactory);
	var converter = new ConversionsManager(parameters.BuildConverterConfig(), factory, loggerFactory);
	var attr = File.GetAttributes(siardFile);
	var files = attr.HasFlag(FileAttributes.Directory) ? Directory.GetFiles(siardFile, "*.siard") : new string[] { siardFile };

	var logger = loggerFactory.CreateLogger("Relational2Rdf");
	foreach (var file in files)
	{
		logger.LogInformation("Converting {0}", file);
		var dataSource = await reader.ReadAsync(file);
		logger.LogDebug("Read datasource {name}, containing {schemas} schemas and {table} tables", dataSource.Name, dataSource.Schemas.Count(), dataSource.Schemas.Sum(x => x.Tables.Count()));
		var outputFile = await converter.ConvertAsync(dataSource);
		await HandleTraceAsync(parameters, outputFile);
		logger.LogInformation("Conversion complete. Output written to {0}", outputFile);
	}
}