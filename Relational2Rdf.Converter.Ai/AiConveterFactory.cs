﻿using AwosFramework.Rdf.Lib.Writer;
using Microsoft.Extensions.Logging;
using Relation2Rdf.Common.Shims;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Ai.Conversion;
using Relational2Rdf.Converter.Ai.Conversion.Settings;
using Relational2Rdf.Converter.Ai.Inference;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Ai
{
	public class AiConveterFactory : IConverterFactory
	{
		private readonly AiConversionSettings _settings;

		private ILoggerFactory _factory;
		private ILogger _logger;
		private ConversionContext? _context;
		private AiMagic? _aiMagic;
		private ITripletWriter? _writer;
		private IRelationalDataSource? _dataSource;
		private bool _initialized = false;

		public AiConveterFactory(AiConversionSettings settings, ILoggerFactory factory)
		{
			_settings = settings;
			_factory=factory;
			_logger = factory.CreateLogger<AiConveterFactory>();
		}

		public Task<ITableConverter> GetTableConverterAsync(ISchema schema, ITable table)
		{
			if(_initialized == false)
				throw new InvalidOperationException($"Factory not initialized, call {nameof(InitAsync)} first");

			var reader = _dataSource!.GetReader(schema, table);
			var res = new TableConverter(_context, _factory, _writer, reader, _settings.TableSettings);
			return Task.FromResult<ITableConverter>(res);
		}

		public async Task InitAsync(ITripletWriter writer, IRelationalDataSource dataSource)
		{
			_writer = writer;
			_dataSource = dataSource;
			if(Enum.TryParse<AiServiceType>(_settings.AiService, true, out var serviceType	) == false)	
				throw new ArgumentException($"Invalid AI service type: {_settings.AiService}, expected one of the following: {string.Join(", ", Enum.GetNames<AiServiceType>())}");

			var aiConfig = new AiConfig(_settings.AiEndpoint, _settings.AiKey, _settings.AiModel, serviceType);
			var inference = InferenceFactory.GetService(aiConfig, _factory);
			_aiMagic = new AiMagic(inference, _factory);
			_context = new ConversionContext(_settings, _aiMagic, dataSource, writer);
			_logger.LogInformation("Initializing AI conversion context");
			await _context.InitAsync();


			// check if foreign keys are present
			var tableCount = dataSource.Schemas.Sum(x => x.Tables.Count());
			var fkCount = dataSource.Schemas.SelectMany(x => x.Tables)
				.Where(x => x.ForeignKeys != null)
				.SelectMany(x => x.ForeignKeys)
				.Count();

			if (_settings.ReconstructMissingRelationships && fkCount == 0 && tableCount > 1)
			{
				_logger.LogInformation("Restoring missing foreign keys");
				using (Profiler.Trace(nameof(AiConveterFactory), "RestoreForeignKeys"))
				{
					dataSource = await ReconstructForeignKeysAsync(dataSource, _aiMagic);
				}
			}

			_initialized = true;
		}

		private static async Task<IRelationalDataSource> ReconstructForeignKeysAsync(IRelationalDataSource dataSource, AiMagic magic)
		{
			var allFKeys = await magic.GuessForeignKeysAsync(dataSource);
			var tables = new List<TableShim>();
			var schemas = new List<SchemaShim>();

			foreach (var schema in dataSource.Schemas)
			{
				tables.Clear();
				foreach (var table in schema.Tables)
				{
					var fKeys = allFKeys.Where(x => x.FromSchema == schema.Name && x.FromTable == table.Name).ToArray();
					tables.Add(new TableShim(table, fKeys));
				}

				schemas.Add(new SchemaShim(schema, tables));
			}

			return new RelationalDatasourceShim(dataSource, schemas);
		}
	}
}
