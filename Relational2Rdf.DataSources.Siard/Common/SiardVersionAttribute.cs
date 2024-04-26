using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.DataSources.Siard.Common
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class SiardVersionAttribute : Attribute
    {
        public SiardVersion Version { get; init; }

        public SiardVersionAttribute(SiardVersion version)
        {
            Version = version;
        }
    }
}
