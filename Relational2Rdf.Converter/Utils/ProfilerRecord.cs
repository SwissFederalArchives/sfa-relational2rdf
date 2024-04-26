using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Utils
{
	public class ProfilerRecord : IDisposable
	{
		public string Category { get; init; }
		public string Unit { get; init; }
		public string Message { get; private set; }
		public TimeSpan Duration { get; private set; }
		private Stopwatch _watch;
		private string _presetMessage;

		public static ProfilerRecord CreatePreset(string category, string unit, string presetMsg)
		{
			var record = new ProfilerRecord
			{
				Category = category,
				Unit = unit,
				_presetMessage = presetMsg
			};
			return record;
		}

		public void Start()
		{
			_watch = new Stopwatch();
			_watch.Start();
		}

		public void Stop(string message)
		{
			_watch.Stop();
			Message = message;
			Duration = _watch.Elapsed;
			_watch = null;
		}

		public void Dispose()
		{
			Stop(_presetMessage);
		}
	}
}
