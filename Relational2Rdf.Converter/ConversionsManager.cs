using AwosFramework.Multithreading.Runners;
using AwosFramework.Rdf.Lib;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Display;
using Relational2Rdf.Converter.Utils;
using System;
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
		private readonly RunnerGroup<SchemaTable, SchemaTable, ConversionEngine> _runnerGroup;
		private readonly ConverterSettings _settings;
		private readonly IConverterFactory _factory;
		private readonly ConsoleDisplay _display;
		private readonly DirectoryInfo _outputFolder;
		private readonly ILogger _logger;
		private readonly Dictionary<Runner<SchemaTable, SchemaTable, ConversionEngine>, ConversionEngine> _engines;

		public ConversionsManager(ConverterSettings settings, IConverterFactory factory, ILoggerFactory loggerFactory)
		{
			_settings = settings;
			_factory = factory;
			_logger = loggerFactory.CreateLogger<ConversionsManager>();
			_runnerGroup = new RunnerGroup<SchemaTable, SchemaTable, ConversionEngine>("Relation2Rdf_Runners", ConvertImpl);
			for (int i = 0; i < _settings.ThreadCount; i++)
			{
				_logger.LogInformation("Creating runner {0}", i);
				var engine = new ConversionEngine(_factory);
				var runner = new Runner<SchemaTable, SchemaTable, ConversionEngine>(_runnerGroup, engine);
			}

			_runnerGroup.OnResult +=_runnerGroup_OnResult;
			_runnerGroup.OnError += _runnerGroup_OnError;	

			if (_settings.ConsoleOutput)
			{
				_display = new ConsoleDisplay(loggerFactory, _runnerGroup.Jobs, _runnerGroup.Runners.Select(x => x.Engine));
				foreach (var engine in _runnerGroup.Runners.Select(x => x.Engine))
				{
					engine.Progress.OnUpdate += (_) => _display?.UpdateEngine(engine);
					engine.Progress.OnSetup += (_) => _display?.UpdateQueue();
				}
			}
		}

		private void _runnerGroup_OnError(object source, SchemaTable input, Exception ex)
		{
			_logger.LogError(ex, "Error converting {0}.{1}", input.Schema.Name, input.Table.Name);
		}

		private void _runnerGroup_OnResult(object sender, SchemaTable result)
		{
			_logger.LogInformation("Converted {0}.{1} successfully", result.Schema.Name, result.Table.Name);
		}

		private SchemaTable ConvertImpl(ConversionEngine engine, SchemaTable table)
		{
			engine.ConvertAsync(table.Schema, table.Table).GetAwaiter().GetResult();
			return table;
		}

		public async Task<string> ConvertAsync(IRelationalDataSource source)
		{
			string outputFile = Path.Join(_settings.OutputDir.FullName, _settings.FileName ?? $"{source.Name.Replace(Path.GetInvalidFileNameChars())}.ttl");
			var writer = WriterFactory.TurtleWriter(outputFile);
			using (Profiler.Trace("InitializeConversionFactory", source.Name))
				await _factory.InitAsync(writer, source);
			
			var jobs = source.Schemas.SelectMany(schema => schema.Tables.Select(table => new SchemaTable(schema, table)));
			_runnerGroup.QueueJobs(jobs);
			_display?.UpdateQueue();
			_runnerGroup.StartAll();
			await _runnerGroup.AwaitAllDone();
			_runnerGroup.StopAll();
			return outputFile;
		}


		public void Dispose()
		{
			_runnerGroup?.Dispose();
		}
	}
}
