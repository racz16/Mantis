namespace Mantis.Core.Logging;

public readonly struct LogEntry
{
	public required LogLevel Level { get; init; }
	public required LogCategory Category { get; init; }
	public required long Timestamp { get; init; }
	public required Thread Thread { get; init; }
	public required string File { get; init; }
	public required string Member { get; init; }
	public required int Line { get; init; }
	public required char[] Message { get; init; }
	public required int MessageLength { get; init; }
}
