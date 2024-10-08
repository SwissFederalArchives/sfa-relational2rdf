﻿using AwosFramework.Rdf.Lib.Writer;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Utils;
using Relational2Rdf.Converter.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter
{
	public class ConversionEngine : ITaskSource<SchemaTable>
	{
		private readonly IConverterFactory _factory;
		public Progress Progress { get; init; }
		public ITable CurrentTable { get; private set; }	

		public ConversionEngine(IConverterFactory factory)
		{
			_factory = factory;
			Progress = new Progress();
		}

		public async Task ConvertAsync(ISchema schema, ITable table)
		{
			CurrentTable = table;
			Progress.Setup(table.RowCount, table.Name);

			var converter = await _factory.GetTableConverterAsync(schema, table);
			await converter.ConvertAsync(Progress);

			Progress.Clear();
			CurrentTable = null;
		}

		public async Task GetTask(SchemaTable job)
		{
			CurrentTable = job.Table;
			Progress.Setup(job.Table.RowCount, job.Table.Name);

			var converter = await _factory.GetTableConverterAsync(job.Schema, job.Table);
			await converter.ConvertAsync(Progress);

			Progress.Clear();
			CurrentTable = null;
		}
	}
}
