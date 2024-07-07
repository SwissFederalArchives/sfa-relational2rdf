using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Utils
{
	public class Progress : IProgress
	{
		public bool Running { get; private set; } = false;
		public int Total { get; private set; }
		public int EventResolution { get; set; } = 1000;
		public string Name { get; private set; }
		public int Current => _current;
		private int _current;

		public double Percentage => Running ? ((double)Current / Total) : 0;
		public event Action<double> OnProgress;
		public event Action<Progress> OnUpdate;
		public event Action<Progress> OnSetup;

		public void Setup(int total, string name = null, int? resolution = null)
		{
			Total = total;
			EventResolution = resolution ?? Math.Max(1, Total / 100);
			Name = name;
			_current = 0;
			Running = true;
			OnUpdate?.Invoke(this);
			OnSetup?.Invoke(this);
		}

		public void Clear()
		{
			Total = 0;
			_current = 0;
			Name = null;
			Running = false;
			OnUpdate?.Invoke(this);
		}

		public void Increment()
		{
			Interlocked.Increment(ref _current);
			if (EventResolution == 1 || _current % EventResolution == 0)
			{
				OnProgress?.Invoke(Percentage);
				OnUpdate?.Invoke(this);
			}
		}

	}
}
