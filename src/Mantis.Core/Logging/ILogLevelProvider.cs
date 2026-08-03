namespace Mantis.Core.Logging;

public interface ILogLevelProvider
{
	static abstract LogLevel Level { get; }
}

#pragma warning disable MA0048 // one file intentionally holds the tiny level marker types

public readonly struct TraceLevel : ILogLevelProvider { public static LogLevel Level => LogLevel.Trace; }
public readonly struct DebugLevel : ILogLevelProvider { public static LogLevel Level => LogLevel.Debug; }
public readonly struct InfoLevel : ILogLevelProvider { public static LogLevel Level => LogLevel.Info; }
public readonly struct WarnLevel : ILogLevelProvider { public static LogLevel Level => LogLevel.Warn; }
public readonly struct ErrorLevel : ILogLevelProvider { public static LogLevel Level => LogLevel.Error; }
public readonly struct FatalLevel : ILogLevelProvider { public static LogLevel Level => LogLevel.Fatal; }
