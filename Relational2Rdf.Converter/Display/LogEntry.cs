using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Relational2Rdf.Converter.Display
{
    public class LogEntry
    {
        public string Name { get; init; }
        public string Header { get; init; }
        public DateTime TimeStamp { get; init; } = DateTime.Now;
        public string Message { get; init; }
        public ConsoleColor Color { get; init; }
        public LogLevel Level { get; init; }

        public LogEntry(string name, string header, string message, ConsoleColor color, LogLevel level)
        {
            Name = name;
            Header = header;
            Message = message.ReplaceLineEndings("\n");
            Color = color;
            Level = level;
        }
    }
}
