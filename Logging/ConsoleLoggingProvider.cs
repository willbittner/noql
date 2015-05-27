using System;

namespace NoQL.CEP.Logging
{
    public class ConsoleLoggingProvider : ILogProvider
    {
        public void LogEvent(LogSeverity Severity, NoQL.CEP.Blocks.AbstractBlock block, object EventObject, string message)
        {
            Console.WriteLine("[{0}][{1}] {2} -- {3}", DateTime.Now, Severity.ToString(), block.DebugName, message);
        }
    }
}