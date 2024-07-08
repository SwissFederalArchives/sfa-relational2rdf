using AwosFramework.Multithreading.Runners;
using AwosFramework.Rdf.Lib;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Display;
using Relational2Rdf.Converter.Utils;
using Relational2Rdf.Converter.Worker;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter
{
	public class ConversionsManager : IDisposable
	{
		private readonly ConverterSettings _settings;
		private readonly IConverterFactory _factory;
		private readonly ConsoleDisplay _display;
		private readonly DirectoryInfo _outputFolder;
		private readonly ILogger _logger;
		private readonly ConversionEngine[] _engines;
		private readonly TaskManager<ConversionEngine, SchemaTable> _taskManager;

		public ConversionsManager(ConverterSettings settings, IConverterFactory factory, ILoggerFactory loggerFactory)
		{
			_settings = settings;
			_factory = factory;
			_logger = loggerFactory.CreateLogger<ConversionsManager>();
			_engines = new ConversionEngine[_settings.ThreadCount];
			for (int i = 0; i < _settings.ThreadCount; i++)
			{
				_logger.LogInformation("Creating runner {0}", i);
				_engines[i] = new ConversionEngine(_factory);
			}

			_taskManager = new TaskManager<ConversionEngine, SchemaTable>(_engines, loggerFactory);
			_taskManager.OnError += _taskManager_OnError;
			_taskManager.OnSuccess += _taskManager_OnSuccess;

			if (_settings.ConsoleOutput)
			{
				_display = new ConsoleDisplay(loggerFactory, _taskManager.Jobs, _engines);
				foreach (var engine in _engines)
				{
					engine.Progress.OnUpdate += (_) => _display?.UpdateEngine(engine);
					engine.Progress.OnSetup += (_) => _display?.UpdateQueue();
				}
			}
		}

		private void _taskManager_OnSuccess(ConversionEngine engine, SchemaTable job)
		{
			_logger.LogInformation("Converted {0}.{1} successfully", job.Schema.Name, job.Table.Name);
		}

		private void _taskManager_OnError(ConversionEngine engine, SchemaTable job, Exception ex)
		{
			_logger.LogError(ex, "Error converting {0}.{1}", job.Schema.Name, job.Table.Name);
		}

		public async Task<string> ConvertAsync(IRelationalDataSource source)
		{
			string outputFile = Path.Join(_settings.OutputDir.FullName, _settings.FileName ?? $"{source.Name.Replace(Path.GetInvalidFileNameChars())}.ttl");
			var writer = WriterFactory.TurtleWriter(outputFile);
			_logger.LogDebug("Created output turtle output file {path} for source {source}", outputFile, source.Name);
			using (Profiler.Trace("InitializeConversionFactory", source.Name))
				await _factory.InitAsync(writer, source);

			var jobs = source.Schemas.SelectMany(schema => schema.Tables.Select(table => new SchemaTable(schema, table)));
			_taskManager.AddJobs(jobs);
			_display?.UpdateQueue();
			await _taskManager.RunAsync();
			writer.Dispose();
			return outputFile;
		}


		public void Dispose()
		{
		}
	}
}
