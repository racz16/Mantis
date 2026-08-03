namespace Mantis.Core.Logging.Sinks;

public interface ILogSink : IDisposable
{
	LogLevel MaximumLevel { get; }
	void Write(in LogEntry entry);
	void Flush();
}
