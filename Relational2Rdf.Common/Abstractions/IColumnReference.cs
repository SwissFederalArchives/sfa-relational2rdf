using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Common.Abstractions
{
    public interface IColumnReference
    {
        public string SourceColumn { get; }
        public string TargetColumn { get; }
    }
}
