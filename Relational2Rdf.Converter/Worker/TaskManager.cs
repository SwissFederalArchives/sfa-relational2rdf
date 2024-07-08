using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Worker
{
	public class TaskManager<TSource, TJob> where TSource : ITaskSource<TJob>
	{
		private readonly TSource[] _sources;
		private readonly Dictionary<Task, (TSource source, TJob job)> _tasks;
		private readonly ConcurrentQueue<TJob> _jobs;
		public IEnumerable<TJob> Jobs => _jobs;
		private readonly ILogger _logger;
		public event Action<TSource, TJob, Exception> OnError;
		public event Action<TSource, TJob> OnSuccess;

		public TaskManager(IEnumerable<TSource> sources, ILoggerFactory factory)
		{
			_logger = factory.CreateLogger<TaskManager<TSource, TJob>>();
			_tasks = new();
			_sources = sources.ToArray();
			_jobs = new ConcurrentQueue<TJob>();
		}

		public void AddJobs(params TJob[] jobs) => AddJobs((IEnumerable<TJob>)jobs);
		public void AddJobs(IEnumerable<TJob> jobs)
		{
			foreach (var job in jobs)
				_jobs.Enqueue(job);

			_logger.LogInformation("{count} jobs enqueue", jobs.Count());
		}

		private void GetTask(TSource source)
		{
			if (_jobs.TryDequeue(out var job))
			{
				var task = Task.Run(() => source.GetTask(job));
				_tasks[task] = (source, job);
				_logger.LogDebug("job {job} assigned to source {source}", job, source);
			}
		}

		private void HandleEvents(Task task, TSource source, TJob job)
		{
			try
			{
				if (task.IsCompletedSuccessfully)
					OnSuccess?.Invoke(source, job);
				else
					OnError?.Invoke(source, job, task.Exception);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "error when firing task completion events");
			}
		}

		public async Task RunAsync()
		{
			foreach (var source in _sources)
				GetTask(source);

			while (_jobs.Count > 0 || _tasks.Count > 0)
			{
				var task = await Task.WhenAny(_tasks.Keys);
				var (source, job) = _tasks[task];
				_tasks.Remove(task);
				HandleEvents(task, source, job);
				GetTask(source);
			}
		}
	}
}
