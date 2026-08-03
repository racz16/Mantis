using System.Globalization;
using System.Text;

namespace Mantis.Core.Logging.Sinks;

public sealed class FileLogSink : ILogSink
{
	public LogLevel MaximumLevel { get; init; } = LogLevel.Debug;
	public bool ShortFilePath { get; init; } = true;

	private readonly StreamWriter writer;

	public FileLogSink(string folder = "log", string fileBase = "Mantis", int bufferSize = LogHelper.DEFAULT_STREAM_WRITER_BUFFER_SIZE)
	{
		string filePath = GetFilePath(folder, fileBase);
		var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);
		var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
		writer = new(stream, encoding, bufferSize) { AutoFlush = false };
	}

	private static string GetFilePath(string folder, string fileBase)
	{
		string fileName = string.Create(CultureInfo.InvariantCulture, $"{fileBase}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log");
		Directory.CreateDirectory(folder);
		return Path.Combine(folder, fileName);
	}

	[SkipLocalsInit]
	public void Write(in LogEntry entry)
	{
		string levelName = entry.Level.GetName();
		var filePath = ShortFilePath ? Path.GetFileName(entry.File.AsSpan()) : entry.File;
		int estimatedCharacterCount = GetEstimatedCharacterCount(in entry, levelName, filePath);
		char[]? characterArray = estimatedCharacterCount > LogHelper.STACK_CHARACTER_LIMIT ? ArrayPool<char>.Shared.Rent(estimatedCharacterCount) : null;
		try
		{
			var characters = characterArray ?? (stackalloc char[estimatedCharacterCount]);
			int position = 0;
			FillCharacters(characters, ref position, in entry, levelName, filePath);
			writer.WriteLine(characters[..position]);
		}
		finally
		{
			if (characterArray is not null)
			{
				ArrayPool<char>.Shared.Return(characterArray);
			}
		}
	}

	private static void FillCharacters(Span<char> characters, ref int position, in LogEntry entry, string levelName, ReadOnlySpan<char> filePath)
	{

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
	}

	private static int GetEstimatedCharacterCount(in LogEntry entry, string levelName, ReadOnlySpan<char> filePath)
	{
		return
			levelName.Length +
			entry.Category.ToString().Length +
			LogHelper.DATE_FORMAT_LENGTH +
			(entry.Thread.Name?.Length ?? 10) +
			filePath.Length +
			entry.Member.Length +
			entry.MessageLength +
			10 + // line
			Environment.NewLine.Length +
			16; // string literals
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
