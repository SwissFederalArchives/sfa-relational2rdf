﻿using Relational2Rdf.Converter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter
{
	public interface ITableConverter
	{
		public Task ConvertAsync(IProgress progress);
	}
}