using System.Text;

namespace Mantis.Core.Logging.Sinks;

public sealed class ConsoleLogSink : ILogSink
{
	private const string NORMAL = "\x1b[39m";
	private const string RED = "\x1b[91m";
	private const string GREEN = "\x1b[92m";
	private const string YELLOW = "\x1b[93m";
	private const string BLUE = "\x1b[94m";
	private const string MAGENTA = "\x1b[95m";
	private const string CYAN = "\x1b[96m";
	private const string GREY = "\x1b[97m";
	private const string BOLD = "\x1b[1m";
	private const string NOBOLD = "\x1b[22m";
	private const string UNDERLINE = "\x1b[4m";
	private const string NOUNDERLINE = "\x1b[24m";
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
		LogHelper.Append(NORMAL + NOREVERSE, characters, ref position);
	}

	private static int GetEstimatedCharacterCount(in LogEntry entry, string levelColor, string levelName, ReadOnlySpan<char> filePath)
	{
		return
			levelColor.Length +
			levelName.Length +
			entry.Category.ToString().Length +
			LogHelper.DATE_FORMAT_LENGTH +
			(entry.Thread.Name?.Length ?? 10) +
			filePath.Length +
			entry.Member.Length +
			entry.MessageLength +
			10 + // line
			Environment.NewLine.Length +
			NORMAL.Length +
			NOREVERSE.Length +
			16; // string literals
	}

	private static string GetLogLevelColor(LogLevel level)
	{
		return level switch
		{
			LogLevel.Fatal => RED + REVERSE,
			LogLevel.Error => RED,
			LogLevel.Warn => YELLOW,
			LogLevel.Info => NORMAL,
			LogLevel.Debug => GREEN,
			LogLevel.Trace => BLUE,
			_ => MAGENTA,
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
