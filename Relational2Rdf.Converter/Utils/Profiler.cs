using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Utils
{
	public static class Profiler
	{
		private static ConcurrentDictionary<string, List<ProfilerRecord>> _records = new ConcurrentDictionary<string, List<ProfilerRecord>>();
		private static SemaphoreSlim _dictLock = new SemaphoreSlim(1, 1);

		public static ProfilerRecord Trace(string category, string unit, string message = null)
		{
			var record = ProfilerRecord.CreatePreset(category, unit, message);
			_dictLock.Wait();
			if (_records.TryGetValue(category, out var list) == false)
			{
				list = new List<ProfilerRecord>();
				_records[category] = list;
			}
			_dictLock.Release();
			list.Add(record);
			record.Start();
			return record;
		}

		public static ProfilerRecord CreateTrace(string category, string unit)
		{
			var record = new ProfilerRecord { Category = category, Unit = unit };
			_dictLock.Wait();
			if(_records.TryGetValue(category, out var list) == false)
			{
				list = new List<ProfilerRecord>();
				_records[category] = list;
			}
			_dictLock.Release();
			list.Add(record);
			return record;
		}

		public static IEnumerable<string> GetCategories() => _records.Keys;

		public static IEnumerable<ProfilerRecord> GetRecords(string category) => _records[category];
	}
}
