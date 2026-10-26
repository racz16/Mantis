using Mantis.Core.Logging;

namespace Mantis.Core;

public static class Time
{
	public static ulong FrameIndex { get; private set; }
	public static long FrameTimestamp { get; private set; }
	public static long RawDeltaTicks { get; private set; }
	public static double RawDeltaTime { get; private set; }
	public static float UnscaledDeltaTime { get; private set; }
	public static float DeltaTime { get; private set; }
	public static double TimeSinceStartup { get; private set; }
	public static double UnscaledTime { get; private set; }
	public static double ScaledTime { get; private set; }

	public static float TimeScale
	{
		get;
		set
		{
			if (!float.IsFinite(value) || value < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(TimeScale)} must be a finite and non-negative number.");
			}
			field = value;
		}
	} = 1f;

	public static float MaximumDeltaTime
	{
		get;
		set
		{
			if (!float.IsFinite(value) || value <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(value), value, $"{nameof(MaximumDeltaTime)} must be a finite and positive number.");
			}
			field = value;
		}
	} = 0.1f;

	private static long startupTimestamp;

	internal static void Initialize()
	{
		Log.Info(LogCategory.Core, "Initializing Time");

		startupTimestamp = Stopwatch.GetTimestamp();
		Reset();
	}

	internal static void Reset()
	{
		FrameTimestamp = Stopwatch.GetTimestamp();
	}

	internal static void BeginFrame()
	{
		long timestamp = Stopwatch.GetTimestamp();
		long rawDeltaTicks = timestamp - FrameTimestamp;
		double rawDeltaTime = rawDeltaTicks / (double)Stopwatch.Frequency;
		float unscaledDeltaTime = (float)Math.Min(rawDeltaTime, MaximumDeltaTime);
		float deltaTime = unscaledDeltaTime * TimeScale;

		FrameIndex++;
		FrameTimestamp = timestamp;
		RawDeltaTicks = rawDeltaTicks;
		RawDeltaTime = rawDeltaTime;
		UnscaledDeltaTime = unscaledDeltaTime;
		DeltaTime = deltaTime;
		TimeSinceStartup = (timestamp - startupTimestamp) / (double)Stopwatch.Frequency;
		UnscaledTime += unscaledDeltaTime;
		ScaledTime += deltaTime;
	}

	internal static void Dispose()
	{
		Log.Info(LogCategory.Core, "Disposing Time");
	}
}
