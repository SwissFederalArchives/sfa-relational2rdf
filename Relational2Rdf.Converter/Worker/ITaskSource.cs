using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Worker
{
	public interface ITaskSource<TJob>
	{
		Task GetTask(TJob job);
	}
}
