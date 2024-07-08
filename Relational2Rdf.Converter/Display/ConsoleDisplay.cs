using AwosFramework.Multithreading.Runners;
using Microsoft.Extensions.Logging;
using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Display
{
	public class ConsoleDisplay : ILoggerProvider
	{
		private readonly SemaphoreSlim _consoleLock = new SemaphoreSlim(1);
		private readonly IEnumerable<SchemaTable> _jobs;
		private readonly ConversionEngine[] _engines;
		private readonly List<LogEntry> _log = new List<LogEntry>();
		private int _lastWidth;
		private int _lastHeight;

		private const int MessageIndent = 4;
		private const int QueueOffset = 1; // Divider
		private const int ProgressOffset = QueueOffset + 2; // Divider + Queue + Divider
		private int LogOffset => ProgressOffset + _engines.Length + 1; // Divider + Queue + Divider + Engines States + Divider 

		public ConsoleDisplay(ILoggerFactory factory, IEnumerable<SchemaTable> jobs, IEnumerable<ConversionEngine> engines)
		{
			factory.AddProvider(this);
			_jobs = jobs;
			_engines = engines.ToArray();
			Console.CursorVisible = false;
			CheckResize();
		}


		public bool CheckResize()
		{
			if (Console.WindowWidth != _lastWidth || Console.WindowHeight != _lastHeight)
			{
				_lastWidth = Console.WindowWidth;
				_lastHeight = Console.WindowHeight;
				RerenderAll();
				return false;
			}

			return true;
		}

		private const string SHADES = "░▒▓█";
		private string RenderProgressBar(string description, double progress)
		{
			if (_lastWidth <= 6)
				return string.Empty;

			if (progress > 1)
				progress = 1;

			var maxBarLen = _lastWidth - 30; // 30 = 2 * 1 divider + 20 chars name + 8 chars percentage
			if (maxBarLen < 0)
			{
				var len = _lastWidth - 6;
				return $"{(description ?? "No Job").Pad(len)}|{progress * 100:000.0}%";
			}

			var virtualFilledLen = (int)(maxBarLen * SHADES.Length * progress);
			var filledLen = virtualFilledLen / SHADES.Length;
			var overShoot = virtualFilledLen % SHADES.Length;
			var bar = new string(SHADES.Last(), filledLen);
			if (overShoot > 0)
				bar += SHADES[overShoot - 1];

			bar += new string(' ', maxBarLen - bar.Length);
			return $"{(description ?? "No Job").Pad(20)}|{bar}| {progress * 100:000.00}%";
		}

		public void RerenderAll()
		{
			Console.CursorTop = 0;
			Console.CursorLeft = 0;
			Console.Write("[Queue]".Pad(_lastWidth, '-'));
			RenderQueue();
			Console.CursorTop = 2;
			Console.CursorLeft = 0;
			Console.Write("[Progress]".Pad(_lastWidth, '-'));
			for (int i = 0; i < _engines.Length; i++)
				RenderProgress(i);
			Console.CursorTop = LogOffset - 1;
			Console.CursorLeft = 0;
			Console.Write("[Log]".Pad(_lastWidth, '-'));
			RenderLog();
		}

		private void RenderLog()
		{
			var foreground = Console.ForegroundColor;
			int position = LogOffset;
			var log = _log.AsEnumerable().Reverse().GetEnumerator();
			while (position < _lastHeight && log.MoveNext())
			{
				Console.CursorTop = position;
				Console.CursorLeft = 0;
				Console.Write($"[{log.Current.TimeStamp:yyMMdd HHmmss}] [");
				Console.ForegroundColor = log.Current.Color;
				Console.Write(log.Current.Level);
				Console.ForegroundColor = foreground;
				Console.Write($"] [{log.Current.Name}]: ");
				var left = _lastWidth - Console.CursorLeft;
				if (left > 0)
					Console.Write(new string(' ', left));

				if (log.Current.Message.Contains("\n"))
				{
					var lines = log.Current.Message.Split("\n");
					Console.ForegroundColor = log.Current.Color;
					position++;
					for (int i = 0; i < lines.Length && position < _lastHeight; i++)
					{
						Console.CursorTop = position++;
						Console.CursorLeft = MessageIndent;
						Console.Write(lines[i].Pad(_lastWidth - MessageIndent));
					}
				}
				else
				{
					Console.ForegroundColor = log.Current.Color;
					Console.CursorTop += 1;
					Console.CursorLeft = MessageIndent;
					Console.Write(log.Current.Message.Pad(_lastWidth - MessageIndent));
					position += 2;
				}

				Console.ForegroundColor = foreground;
			}
		}

		public void UpdateLog(LogEntry entry)
		{
			_log.Add(entry);
			if (_log.Count > Console.LargestWindowHeight)
				_log.RemoveAt(0);

			_consoleLock.Wait();
			if (CheckResize())
				RenderLog();
			_consoleLock.Release();
		}

		private void RenderProgress(int index)
		{
			var engine = _engines[index];
			Console.CursorTop = ProgressOffset + index;
			Console.CursorLeft = 0;
			Console.Write(RenderProgressBar(engine.Progress.Name, engine.Progress.Percentage));
		}

		public void UpdateEngine(ConversionEngine sender)
		{
			var index = Array.IndexOf(_engines, sender);
			if (index < 0)
				return;

			if (_consoleLock.Wait(1000) == false)
				return;

			if (CheckResize())
				RenderProgress(index);

			_consoleLock.Release();
		}

		private void RenderQueue()
		{
			Console.CursorTop = 1;
			Console.CursorLeft = 0;
			Console.Write($"{_jobs.Count():000}|{string.Join(", ", _jobs.Select(x => x.Table.Name))}".Pad(_lastWidth));
		}

		public void UpdateQueue()
		{
			_consoleLock.Wait();
			if (CheckResize())
				RenderQueue();

			_consoleLock.Release();
		}

		private ColorConsoleLoggerConfiguration _currentConfig = new ColorConsoleLoggerConfiguration();
		private readonly ConcurrentDictionary<string, ColorConsoleDisplayLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

		public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new ColorConsoleDisplayLogger(name, this, _currentConfig));


		public void Dispose()
		{
			_loggers.Clear();
			Console.CursorVisible = true;
		}
	}
}
