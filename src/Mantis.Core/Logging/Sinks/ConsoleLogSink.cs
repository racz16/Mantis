using System.Text;

namespace Mantis.Core.Logging.Sinks;

public sealed class ConsoleLogSink : ILogSink
{
	private const string DEFAULT = "\x1b[39m";
	private const string BRIGHT_BLACK = "\x1b[90m";
	private const string BRIGHT_RED = "\x1b[91m";
	private const string BRIGHT_GREEN = "\x1b[92m";
	private const string BRIGHT_YELLOW = "\x1b[93m";
	private const string BRIGHT_MAGENTA = "\x1b[95m";
	private const string BRIGHT_CYAN = "\x1b[96m";
	private const string REVERSE = "\x1b[7m";
	private const string NOREVERSE = "\x1b[27m";

	public LogLevel MaximumLevel { get; init; } = LogLevel.Debug;
	public bool ShortFilePath { get; init; } = true;

	private readonly StreamWriter writer;

	public ConsoleLogSink(int bufferSize = LogHelper.DEFAULT_STREAM_WRITER_BUFFER_SIZE)
	{
		Console.OutputEncoding = Encoding.UTF8;
		writer = new(Console.OpenStandardOutput(), new UTF8Encoding(false), bufferSize) { AutoFlush = false };
	}

	[SkipLocalsInit]
	public void Write(in LogEntry entry)
	{
		string levelName = entry.Level.GetName();
		string levelColor = GetLogLevelColor(entry.Level);
		var filePath = ShortFilePath ? Path.GetFileName(entry.File.AsSpan()) : entry.File;
		int estimatedCharacterCount = GetEstimatedCharacterCount(in entry, levelColor, levelName, filePath);
		char[]? characterArray = estimatedCharacterCount > LogHelper.STACK_CHARACTER_LIMIT ? ArrayPool<char>.Shared.Rent(estimatedCharacterCount) : null;
		try
		{
			var characters = characterArray ?? (stackalloc char[estimatedCharacterCount]);
			int position = 0;
			FillCharacters(characters, ref position, in entry, levelColor, levelName, filePath);
			writer.WriteLine(characters[..position]);
		}
		finally
		{
			if (characterArray != null)
			{
				ArrayPool<char>.Shared.Return(characterArray);
			}
		}
	}

	private static void FillCharacters(Span<char> characters, ref int position, in LogEntry entry, string levelColor, string levelName, ReadOnlySpan<char> filePath)
	{
		LogHelper.Append(levelColor, characters, ref position);
		LogHelper.Append("[", characters, ref position);
		LogHelper.Append(levelName, characters, ref position);
		LogHelper.Append("] [", characters, ref position);
		LogHelper.Append(entry.Category.ToString(), characters, ref position);
		LogHelper.Append("] [", characters, ref position);
		LogHelper.Append(LogHelper.ToDateTimeUtc(entry.Timestamp), characters, ref position, LogHelper.DATE_FORMAT);
		LogHelper.Append("] [", characters, ref position);
		LogHelper.Append(entry.FrameIndex, characters, ref position);
		LogHelper.Append("] [", characters, ref position);
		ReadOnlySpan<char> threadName = entry.Thread.Name;
		if (threadName.IsEmpty)
		{
			LogHelper.Append(entry.Thread.ManagedThreadId, characters, ref position);
		}
		else
		{
			LogHelper.Append(threadName, characters, ref position);
		}
		LogHelper.Append("] [", characters, ref position);
		LogHelper.Append(filePath, characters, ref position);
		LogHelper.Append(":", characters, ref position);
		LogHelper.Append(entry.Member, characters, ref position);
		LogHelper.Append(":", characters, ref position);
		LogHelper.Append(entry.Line, characters, ref position);
		LogHelper.Append("]", characters, ref position);
		LogHelper.Append(Environment.NewLine, characters, ref position);
		LogHelper.Append(entry.Message.AsSpan(0, entry.MessageLength), characters, ref position);
		LogHelper.Append(DEFAULT + NOREVERSE, characters, ref position);
	}

	private static int GetEstimatedCharacterCount(in LogEntry entry, string levelColor, string levelName, ReadOnlySpan<char> filePath)
	{
		return
			levelColor.Length +
			levelName.Length +
			entry.Category.ToString().Length +
			LogHelper.DATE_FORMAT_LENGTH +
			20 + // frame index
			(entry.Thread.Name?.Length ?? 10) +
			filePath.Length +
			entry.Member.Length +
			entry.MessageLength +
			10 + // line
			Environment.NewLine.Length +
			DEFAULT.Length +
			NOREVERSE.Length +
			19; // string literals
	}

	private static string GetLogLevelColor(LogLevel level)
	{
		return level switch
		{
			LogLevel.Fatal => BRIGHT_RED + REVERSE,
			LogLevel.Error => BRIGHT_RED,
			LogLevel.Warn => BRIGHT_YELLOW,
			LogLevel.Info => BRIGHT_GREEN,
			LogLevel.Debug => BRIGHT_CYAN,
			LogLevel.Trace => BRIGHT_BLACK,
			_ => BRIGHT_MAGENTA,
		};
	}

	public void Flush()
	{
		writer.Flush();
	}

	public void Dispose()
	{
		Flush();
		writer.Dispose();
		GC.SuppressFinalize(this);
	}
}
