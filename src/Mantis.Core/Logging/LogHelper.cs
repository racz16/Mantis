using System.Globalization;

namespace Mantis.Core.Logging;

public static class LogHelper
{
	public const string DATE_FORMAT = "yyyy-MM-dd HH':'mm':'ss'.'ffffff";
	public const int DATE_FORMAT_LENGTH = 26;
	public const int STACK_CHARACTER_LIMIT = 256;
	public const int DEFAULT_STREAM_WRITER_BUFFER_SIZE = 4096;

	private static readonly long epochTimestamp = Stopwatch.GetTimestamp();
	private static readonly DateTime epochUtc = DateTime.UtcNow;

	public static void Append(ReadOnlySpan<char> from, Span<char> to, ref int position)
	{
		from.CopyTo(to[position..]);
		position += from.Length;
	}

	public static void Append(ISpanFormattable from, Span<char> to, ref int position, ReadOnlySpan<char> format = default)
	{
		bool result = from.TryFormat(to[position..], out int written, format, CultureInfo.InvariantCulture);
		Debug.Assert(result);
		position += written;
	}

	public static string GetName(this LogLevel level)
	{
		return level switch
		{
			LogLevel.Fatal => nameof(LogLevel.Fatal),
			LogLevel.Error => nameof(LogLevel.Error),
			LogLevel.Warn => nameof(LogLevel.Warn),
			LogLevel.Info => nameof(LogLevel.Info),
			LogLevel.Debug => nameof(LogLevel.Debug),
			LogLevel.Trace => nameof(LogLevel.Trace),
			_ => "UNKNOWN",
		};
	}

	public static DateTime ToDateTimeUtc(long timestamp)
	{
		return epochUtc + Stopwatch.GetElapsedTime(epochTimestamp, timestamp);
	}
}
