namespace Mantis.Core;

public static class MantisEngine
{
	public const string NAME = "Mantis";
	public static Version Version { get; } = new(0, 1, 0);

	public const string MANTIS_LOG_TRACE = nameof(MANTIS_LOG_TRACE);
	public const string MANTIS_LOG_DEBUG = nameof(MANTIS_LOG_DEBUG);
	public const string MANTIS_LOG_INFO = nameof(MANTIS_LOG_INFO);
	public const string MANTIS_LOG_WARN = nameof(MANTIS_LOG_WARN);
	public const string MANTIS_LOG_ERROR = nameof(MANTIS_LOG_ERROR);
	public const string MANTIS_LOG_FATAL = nameof(MANTIS_LOG_FATAL);
}
