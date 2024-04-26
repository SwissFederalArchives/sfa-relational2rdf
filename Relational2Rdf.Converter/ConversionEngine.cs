using AwosFramework.Rdf.Lib.Writer;
using Relational2Rdf.Common.Abstractions;
using Relational2Rdf.Converter.Conversion;
using Relational2Rdf.Converter.Conversion.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter
{
	internal class ConversionEngine
	{
		private ConversionContext _context;
		private ITripletWriter _writer;
		private ConversionSettings _settings;

		public ITable CurrentTable { get; private set; }
		public int Rows { get; private set; }
		public int Current { get; private set; }
		public double Progress => (double)Current / Rows;

		public event Action<ConversionEngine> OnProgress;

		public ConversionEngine(ConversionContext context, ITripletWriter writer, ConversionSettings setting)
		{
			_context=context;
			_writer=writer;
			_settings=setting;
		}

		public async Task<bool> ConvertAsync(ISchema schema, ITable table)
		{
			Rows = table.RowCount;
			Current = 0;
			CurrentTable = table;
			using var reader = _context.DataSource.GetReader(schema, table);
			var tableConverter = new TableConverter(_context, _writer, reader, _settings.TableSettings);
			await tableConverter.ConvertAsync((prog) =>
			{
				Current = prog;
				OnProgress?.Invoke(this);
			});

			Current = table.RowCount;
			OnProgress?.Invoke(this);
			//CurrentTable = null;
			return true;
		}
	}
}
