using AwosFramework.Factories;
using AwosFramework.Multithreading.Runners;
using AwosFramework.Rdf.Lib;
using AwosFramework.Rdf.Lib.Writer;
using Microsoft.Extensions.Options;
using Relation2Rdf.Common.Shims;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Conversion;
using Relational2Rdf.Converter.Conversion.Settings;
using Relational2Rdf.Converter.Utils;
using Relational2Rdf.DataSources.Siard;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter
{
	public static class Converter
	{
		private static List<Runner<(ISchema, ITable), bool, ConversionEngine>> _runners = new List<Runner<(ISchema, ITable), bool, ConversionEngine>>();
		private static List<ConversionEngine> _engines = new List<ConversionEngine>();
		private static RunnerGroup<(ISchema, ITable), bool, ConversionEngine> _group;
		private static AutoResetEvent _updateTrigger = new AutoResetEvent(false);
		private static int _converted;
		private static int _failed;

		private static void setupRunners(int runnerCount, ConversionContext context, ITripletWriter writer, ConversionSettings settings)
		{
			_group = new RunnerGroup<(ISchema, ITable), bool, ConversionEngine>("ConversionEngineRunners", (engine, input) => engine.ConvertAsync(input.Item1, input.Item2).Result);
			for (int i = 0; i < runnerCount; i++)
			{
				var engine = new ConversionEngine(context, writer, settings);
				engine.OnProgress += onProgress;
				_engines.Add(engine);
				var runner = new Runner<(ISchema, ITable), bool, ConversionEngine>(_group, engine);
				_runners.Add(runner);
			}
			_group.StartAll();
		}

		private static void onProgress(ConversionEngine engine)
		{
			_updateTrigger.Set();
		}

		private static void groupOnResult(object sender, bool job)
		{
			Interlocked.Increment(ref _converted);
			_updateTrigger.Set();
		}

		private static void groupOnError(object source, (ISchema schema, ITable table) job, Exception ex)
		{
			Interlocked.Increment(ref _failed);
			_updateTrigger.Set();
		}

		private const string SHADES = "░▒▓█";
		private static string renderEngineState(ConversionEngine engine, int maxWidth)
		{
			var maxBarLen = maxWidth - 30; // 30 = 2 * 1 divider + 20 chars name + 8 chars percentage
			if (engine.CurrentTable == null)
				return "No Job".Pad(20);

			var percentage = engine.Progress;
			var virtualFilledLen = (int)((maxBarLen*SHADES.Length)*percentage);
			var filledLen = virtualFilledLen / SHADES.Length;
			var overShoot = virtualFilledLen % SHADES.Length;
			var bar = new string(SHADES.Last(), filledLen);
			if (overShoot > 0)
				bar += SHADES[overShoot-1];

			bar += new string(' ', maxBarLen - bar.Length);
			return $"{engine.CurrentTable.Name.Pad(20)}|{bar}| {(percentage*100):000.00}%";
		}

		private static void updateDisplay(int top, int tableCount, bool quiet)
		{
			if (quiet)
				return;

			var builder = new StringBuilder();
			builder.AppendLine($"Processed Tables: {_converted}/{tableCount}");
			builder.AppendLine(new string('-', Console.WindowWidth-1));
			for (int i = 0; i < _engines.Count; i++)
			{
				var engine = _engines[i];
				builder.AppendLine(renderEngineState(engine, Console.WindowWidth - 1));
			}
			Console.SetCursorPosition(0, top+1);
			Console.Write(builder.ToString());
		}

		private static async Task<IRelationalDataSource> ReconstructForeignKeysAsync(IRelationalDataSource dataSource, AiMagic magic)
		{
			var allFKeys = await magic.GuessForeignKeysAsync(dataSource);
			var tables = new List<TableShim>();
			var schemas = new List<SchemaShim>();

			foreach(var schema in dataSource.Schemas)
			{
				tables.Clear();
				foreach(var table in schema.Tables)
				{
					var fKeys = allFKeys.Where(x => x.FromSchema == schema.Name && x.FromTable == table.Name).ToArray();
					tables.Add(new TableShim(table, fKeys));
				}

				schemas.Add(new SchemaShim(schema, tables));
			}

			return new RelationalDatasourceShim(dataSource, schemas);
		}

		public static async Task ConvertAsync(FileInfo file, DirectoryInfo output, ConversionSettings settings)
		{
			var reader = new SiardFileReader();
			var archive = await reader.ReadAsync(file.FullName);
			var writer = WriterFactory.TurtleWriter(Path.Join(output.FullName, file.Name + ".ttl"));
			var quiet = false;

			var top = Console.CursorTop;
			var tableCount = archive.Schemas.Sum(x => x.Tables.Count());
			var threadCount = Math.Min(settings.ThreadCount, tableCount);

			var aiMagic = new AiMagic(settings.AiKey, settings.AiModel, settings.AiEndpoint);


			// check if foreign keys are present
			var fkCount = archive.Schemas.SelectMany(x => x.Tables).SelectMany(x => x.ForeignKeys).Count();
			if (fkCount == 0 && tableCount > 1)
			{
				using(Profiler.Trace(nameof(Converter), "RestoreForeignKeys"))
				{
					archive = await ReconstructForeignKeysAsync(archive, aiMagic);
				}
			}

			using (Profiler.Trace(nameof(Converter), "SetupContext"))
			{
				var context = new ConversionContext(settings, aiMagic, archive, writer);
				await context.InitAsync();
				setupRunners(threadCount, context, writer, settings);
			}

			var toConvert = archive.Schemas.SelectMany(x => x.Tables.Select(y => (x, y))).ToList();
			_group.OnResult += groupOnResult;
			_group.OnError += groupOnError;
			_group.QueueJobs(toConvert);
			Console.CursorVisible = false;
			using (Profiler.Trace(nameof(Converter), "Conversion"))
			{
				while (_converted + _failed < toConvert.Count)
				{
					_updateTrigger.WaitOne();
					updateDisplay(top, toConvert.Count, quiet);
				}
			}
			updateDisplay(top, toConvert.Count, quiet);
			Console.SetCursorPosition(0, top+_engines.Count+3);
			Console.CursorVisible = true;

			archive.Dispose();
			writer.Dispose();
			_group.StopAll();
		}
	}
}
