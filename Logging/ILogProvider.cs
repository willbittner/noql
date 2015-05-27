using NoQL.CEP.Blocks;

namespace NoQL.CEP.Logging
{
    public enum LogSeverity
    {
        SEVERE = 3,
        NOTICE = 2,
        INFO = 1
    }

    public interface ILogProvider
    {
        void LogEvent(LogSeverity Severity, AbstractBlock block, object EventObject, string message);
    }
}