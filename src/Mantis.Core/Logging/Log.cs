using System.Threading.Channels;

using Mantis.Core.Logging.Sinks;

namespace Mantis.Core.Logging;

public static class Log
{
	public static LogLevel MaximumLevel
	{
		get => maximumLevel;
		set => maximumLevel = value;
	}

	private const int DEFAULT_CHANNEL_BUFFER_SIZE = 1024;
	private const LogLevel NO_OVERRIDE = default;

	private static volatile bool initialized;
	private static bool disposed;
	private static volatile Channel<LogEntry> entries = CreateChannel(DEFAULT_CHANNEL_BUFFER_SIZE);
	private static readonly List<ILogSink> sinks = [];
	private static Thread? thread;
	private static volatile LogLevel[] categoryMaximumLevels = new LogLevel[4];
	private static readonly Lock categoryMaximumLevelsLock = new();
	private static volatile LogLevel maximumLevel = LogLevel.Debug;
	private static int droppedEntryCount;

	public static void Initialize()
	{
		System.Diagnostics.Debug.Assert(!initialized);
		System.Diagnostics.Debug.Assert(!disposed);
		AddDefaultSinks();
		initialized = true;
		CreateAndStartThread();
	}

	public static void Initialize(in LogInitializationDescriptor descriptor)
	{
		System.Diagnostics.Debug.Assert(!initialized);
		System.Diagnostics.Debug.Assert(!disposed);
		if (descriptor.BufferSize.HasValue && descriptor.BufferSize.Value != DEFAULT_CHANNEL_BUFFER_SIZE)
		{
			System.Diagnostics.Debug.Assert(descriptor.BufferSize.Value > 0);
			entries = CreateChannel(descriptor.BufferSize.Value);
		}
		if (descriptor.MaximumLevel.HasValue)
		{
			MaximumLevel = descriptor.MaximumLevel.Value;
		}
		if (descriptor.CategoryMaximumLevels != null)
		{
			AddCategoryMaximumLevels(descriptor.CategoryMaximumLevels);
		}
		if (descriptor.Sinks != null)
		{
			sinks.AddRange(descriptor.Sinks);
		}
		else
		{
			AddDefaultSinks();
		}
		initialized = true;
		CreateAndStartThread();
	}

	private static Channel<LogEntry> CreateChannel(int bufferSize)
	{
		return Channel.CreateBounded<LogEntry>(
			new BoundedChannelOptions(bufferSize)
			{
				SingleReader = true,
				FullMode = BoundedChannelFullMode.DropOldest,
			},
			static droppedEntry =>
			{
				_ = Interlocked.Increment(ref droppedEntryCount);
				ArrayPool<char>.Shared.Return(droppedEntry.Message);
			});
	}

	private static void AddCategoryMaximumLevels(IDictionary<LogCategory, LogLevel> overrides)
	{
		int arraySize = 4;
		foreach (var category in overrides.Keys)
		{
			if (category.Id >= arraySize)
			{
				arraySize = category.Id + 1;
			}
		}
		lock (categoryMaximumLevelsLock)
		{
			if (arraySize > 4)
			{
				categoryMaximumLevels = new LogLevel[arraySize];
			}
			foreach (var over in overrides)
			{
				categoryMaximumLevels[over.Key.Id] = over.Value;
			}
		}
	}

	private static void AddDefaultSinks()
	{
		sinks.Add(new ConsoleLogSink());
	}

	private static void CreateAndStartThread()
	{
		thread = new Thread(LogHandling)
		{
			Name = "Mantis Logging thread",
			Priority = ThreadPriority.BelowNormal,
			IsBackground = true,
		};
		thread.Start();
		AppDomain.CurrentDomain.ProcessExit += static (_, _) => Dispose();
	}

	private static void LogHandling()
	{
		while (WaitToRead())
		{
			while (entries.Reader.TryRead(out var entry))
			{
				WriteEntryToSinks(in entry);
			}
			int dropped = Interlocked.Exchange(ref droppedEntryCount, 0);
			if (dropped > 0)
			{
				LogDroppedMessageCount(LogCategory.Core, $"{dropped} messages dropped");
			}
			FlushSinks();
		}

		DisposeSinks();
	}

	private static void LogDroppedMessageCount(
		LogCategory category,
		[InterpolatedStringHandlerArgument(nameof(category))] scoped ref LogInterpolatedStringHandler<WarnLevel> interpolationHandler,
		[CallerFilePath] string file = "",
		[CallerMemberName] string member = "",
		[CallerLineNumber] int line = 0)
	{
		char[]? message = interpolationHandler.Message;
		if (message == null)
		{
			return;
		}
		try
		{
			var entry = new LogEntry()
			{
				Level = LogLevel.Warn,
				Category = category,
				Timestamp = Stopwatch.GetTimestamp(),
				Thread = Thread.CurrentThread,
				File = file,
				Member = member,
				Line = line,
				Message = message,
				MessageLength = interpolationHandler.MessageLength,
			};
			WriteEntryToSinks(in entry);
		}
		catch
		{
			ArrayPool<char>.Shared.Return(message);
		}
	}

	private static bool WaitToRead()
	{
		var wait = entries.Reader.WaitToReadAsync();
		return wait.IsCompletedSuccessfully ? wait.Result : wait.AsTask().GetAwaiter().GetResult();
	}

	private static void WriteEntryToSinks(in LogEntry entry)
	{
		try
		{
			foreach (var sink in sinks)
			{
				try
				{
					if (entry.Level <= sink.MaximumLevel)
					{
						sink.Write(in entry);
					}
				}
				catch (Exception ex)
				{
					// An exception in a sink can't break the whole logging
					System.Diagnostics.Debug.WriteLine($"Exception while writing into the sink: {ex}");
				}
			}
		}
		finally
		{
			ArrayPool<char>.Shared.Return(entry.Message);
		}
	}

	private static void FlushSinks()
	{
		foreach (var sink in sinks)
		{
			try
			{
				sink.Flush();
			}
			catch (Exception ex)
			{
				// An exception in a sink can't break the whole logging
				System.Diagnostics.Debug.WriteLine($"Exception while flushing the sink: {ex}");
			}
		}
	}

	private static void DisposeSinks()
	{
		foreach (var sink in sinks)
		{
			try
			{
				sink.Dispose();
			}
			catch (Exception ex)
			{
				// An exception in a sink Dispose can't prevent other sinks' Dispose to be called
				System.Diagnostics.Debug.WriteLine($"Exception while disposing the sink: {ex}");
			}
		}
	}

	public static void SetCategoryMaximumLevel(LogCategory category, LogLevel? level)
	{
		System.Diagnostics.Debug.Assert(initialized);
		System.Diagnostics.Debug.Assert(!disposed);
		lock (categoryMaximumLevelsLock)
		{
			if (category.Id >= categoryMaximumLevels.Length)
			{
				int newLength = Math.Max(categoryMaximumLevels.Length * 2, category.Id + 1);
				var newCategoryOverrides = new LogLevel[newLength];
				categoryMaximumLevels.CopyTo(newCategoryOverrides);
				categoryMaximumLevels = newCategoryOverrides;
			}
			categoryMaximumLevels[category.Id] = level ?? NO_OVERRIDE;
		}
	}

	public static bool IsEnabled(LogLevel level, LogCategory category)
	{
		System.Diagnostics.Debug.Assert(initialized);
		var overrides = categoryMaximumLevels;
		uint id = (uint)category.Id;
		var max = id < (uint)overrides.Length ? overrides[id] : NO_OVERRIDE;
		if (max == NO_OVERRIDE)
		{
			max = maximumLevel;
		}
		return level <= max;
	}

	#region Trace

	public static void Trace<T>(LogCategory category, T instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : ISpanFormattable
	{
		Trace(category, $"{instance}", file, member, line);
	}

	public static void Trace(
		LogCategory category,
		[InterpolatedStringHandlerArgument(nameof(category))] scoped ref LogInterpolatedStringHandler<TraceLevel> interpolationHandler,
		[CallerFilePath] string file = "",
		[CallerMemberName] string member = "",
		[CallerLineNumber] int line = 0)
	{
		LogInterpolatedString(LogLevel.Trace, category, interpolationHandler.Message, interpolationHandler.MessageLength, file, member, line);
	}

	public static void Trace(LogCategory category, string message, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		LogString(LogLevel.Trace, category, message, file, member, line);
	}

	public static void Trace(LogCategory category, object? instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		Trace(category, $"{instance}", file, member, line);
	}

	#endregion

	#region Debug

	public static void Debug<T>(LogCategory category, T instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : ISpanFormattable
	{
		Debug(category, $"{instance}", file, member, line);
	}

	public static void Debug(
		LogCategory category,
		[InterpolatedStringHandlerArgument(nameof(category))] scoped ref LogInterpolatedStringHandler<DebugLevel> interpolationHandler,
		[CallerFilePath] string file = "",
		[CallerMemberName] string member = "",
		[CallerLineNumber] int line = 0)
	{
		LogInterpolatedString(LogLevel.Debug, category, interpolationHandler.Message, interpolationHandler.MessageLength, file, member, line);
	}

	public static void Debug(LogCategory category, string message, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		LogString(LogLevel.Debug, category, message, file, member, line);
	}

	public static void Debug(LogCategory category, object? instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		Debug(category, $"{instance}", file, member, line);
	}

	#endregion

	#region Info

	public static void Info<T>(LogCategory category, T instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : ISpanFormattable
	{
		Info(category, $"{instance}", file, member, line);
	}

	public static void Info(
		LogCategory category,
		[InterpolatedStringHandlerArgument(nameof(category))] scoped ref LogInterpolatedStringHandler<InfoLevel> interpolationHandler,
		[CallerFilePath] string file = "",
		[CallerMemberName] string member = "",
		[CallerLineNumber] int line = 0)
	{
		LogInterpolatedString(LogLevel.Info, category, interpolationHandler.Message, interpolationHandler.MessageLength, file, member, line);
	}

	public static void Info(LogCategory category, string message, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		LogString(LogLevel.Info, category, message, file, member, line);
	}

	public static void Info(LogCategory category, object? instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		Info(category, $"{instance}", file, member, line);
	}

	#endregion

	#region Warn

	public static void Warn<T>(LogCategory category, T instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : ISpanFormattable
	{
		Warn(category, $"{instance}", file, member, line);
	}

	public static void Warn(
		LogCategory category,
		[InterpolatedStringHandlerArgument(nameof(category))] scoped ref LogInterpolatedStringHandler<WarnLevel> interpolationHandler,
		[CallerFilePath] string file = "",
		[CallerMemberName] string member = "",
		[CallerLineNumber] int line = 0)
	{
		LogInterpolatedString(LogLevel.Warn, category, interpolationHandler.Message, interpolationHandler.MessageLength, file, member, line);
	}

	public static void Warn(LogCategory category, string message, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		LogString(LogLevel.Warn, category, message, file, member, line);
	}

	public static void Warn(LogCategory category, object? instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		Warn(category, $"{instance}", file, member, line);
	}

	#endregion

	#region Error

	public static void Error<T>(LogCategory category, T instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : ISpanFormattable
	{
		Error(category, $"{instance}", file, member, line);
	}

	public static void Error(
		LogCategory category,
		[InterpolatedStringHandlerArgument(nameof(category))] scoped ref LogInterpolatedStringHandler<ErrorLevel> interpolationHandler,
		[CallerFilePath] string file = "",
		[CallerMemberName] string member = "",
		[CallerLineNumber] int line = 0)
	{
		LogInterpolatedString(LogLevel.Error, category, interpolationHandler.Message, interpolationHandler.MessageLength, file, member, line);
	}

	public static void Error(LogCategory category, string message, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		LogString(LogLevel.Error, category, message, file, member, line);
	}

	public static void Error(LogCategory category, object? instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		Error(category, $"{instance}", file, member, line);
	}

	#endregion

	#region Fatal

	public static void Fatal<T>(LogCategory category, T instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : ISpanFormattable
	{
		Fatal(category, $"{instance}", file, member, line);
	}

	public static void Fatal(
		LogCategory category,
		[InterpolatedStringHandlerArgument(nameof(category))] scoped ref LogInterpolatedStringHandler<FatalLevel> interpolationHandler,
		[CallerFilePath] string file = "",
		[CallerMemberName] string member = "",
		[CallerLineNumber] int line = 0)
	{
		LogInterpolatedString(LogLevel.Fatal, category, interpolationHandler.Message, interpolationHandler.MessageLength, file, member, line);
	}

	public static void Fatal(LogCategory category, string message, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		LogString(LogLevel.Fatal, category, message, file, member, line);
	}

	public static void Fatal(LogCategory category, object? instance, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		Fatal(category, $"{instance}", file, member, line);
	}

	#endregion

	private static void LogString(LogLevel level, LogCategory category, string message, string file, string member, int line)
	{
		System.Diagnostics.Debug.Assert(initialized);
		System.Diagnostics.Debug.Assert(!disposed);
		if (!IsEnabled(level, category))
		{
			return;
		}
		char[] characters = ArrayPool<char>.Shared.Rent(message.Length);
		try
		{
			message.CopyTo(characters);
			Enqueue(level, category, characters, message.Length, file, member, line);
		}
		catch
		{
			ArrayPool<char>.Shared.Return(characters);
		}
	}

	private static void LogInterpolatedString(LogLevel level, LogCategory category, char[]? message, int messageLength, string file, string member, int line)
	{
		System.Diagnostics.Debug.Assert(initialized);
		System.Diagnostics.Debug.Assert(!disposed);
		if (message == null)
		{
			return;
		}
		try
		{
			Enqueue(level, category, message, messageLength, file, member, line);
		}
		catch
		{
			ArrayPool<char>.Shared.Return(message);
		}
	}

	private static void Enqueue(LogLevel level, LogCategory category, char[] message, int messageLength, string file, string member, int line)
	{
		var entry = new LogEntry()
		{
			Level = level,
			Category = category,
			Timestamp = Stopwatch.GetTimestamp(),
			Thread = Thread.CurrentThread,
			File = file,
			Member = member,
			Line = line,
			Message = message,
			MessageLength = messageLength,
		};
		bool written = entries.Writer.TryWrite(entry);
		if (!written)
		{
			ArrayPool<char>.Shared.Return(message);
		}
	}

	public static void Dispose()
	{
		if (Interlocked.Exchange(ref disposed, true))
		{
			return;
		}
		_ = entries.Writer.TryComplete();
		bool? joined = thread?.Join(TimeSpan.FromSeconds(5));
		if (joined.HasValue && !joined.Value)
		{
			System.Diagnostics.Debug.WriteLine("Disposing the logging system timed out");
		}
	}
}
