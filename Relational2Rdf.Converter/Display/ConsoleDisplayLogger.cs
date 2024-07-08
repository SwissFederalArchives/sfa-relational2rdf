using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Display
{
	public sealed class ColorConsoleLoggerConfiguration
	{
		public int EventId { get; set; }

		public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
		{
			[LogLevel.Information] = ConsoleColor.White,
			[LogLevel.Warning] = ConsoleColor.Yellow,
			[LogLevel.Debug] = ConsoleColor.Blue,
			[LogLevel.Error] = ConsoleColor.Red,
			[LogLevel.Critical] = ConsoleColor.DarkRed,
			[LogLevel.Trace] = ConsoleColor.Green
		};
	}

	public sealed class ColorConsoleDisplayLogger() : ILogger
	{

		public string Name { get; init; }
		public ConsoleDisplay Display { get; init; }
		private readonly ColorConsoleLoggerConfiguration _config;
		public LogLevel MinLogLevel { get; set; } = LogLevel.Information;

		public ColorConsoleDisplayLogger(string name, ConsoleDisplay display, ColorConsoleLoggerConfiguration config) : this()
		{
			Name = name;
			Display = display;
			_config = config;
		}

		public IDisposable BeginScope<TState>(TState state) where TState : notnull => default!;

		public bool IsEnabled(LogLevel logLevel) => logLevel >= MinLogLevel && _config.LogLevelToColorMap.ContainsKey(logLevel);

		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (!IsEnabled(logLevel))
			{
				return;
			}

			if (_config.EventId == 0 || _config.EventId == eventId.Id)
			{
				ConsoleColor originalColor = Console.ForegroundColor;
				var entry = new LogEntry(Name, $"[{eventId.Id,2}: {logLevel,-12}]", formatter(state, exception), _config.LogLevelToColorMap[logLevel], logLevel);
				Display.UpdateLog(entry);
			}
		}
	}
}
