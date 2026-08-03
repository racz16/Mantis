using Mantis.Core.Logging.Sinks;

namespace Mantis.Core.Logging;

public readonly ref struct LogInitializationDescriptor
{
	public LogLevel? MaximumLevel { get; init; }
	public IDictionary<LogCategory, LogLevel>? CategoryMaximumLevels { get; init; }
	public IEnumerable<ILogSink>? Sinks { get; init; }
	public int? BufferSize { get; init; }
}
