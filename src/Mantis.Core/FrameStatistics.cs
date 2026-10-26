using Mantis.Core.Logging;

namespace Mantis.Core;

public static class FrameStatistics
{
	public static double FramesPerSecond { get; private set; }
	public static double AverageFrameTime { get; private set; }
	public static double MinimumFrameTime { get; private set; }
	public static double MaximumFrameTime { get; private set; }
	public static long AllocatedBytes { get; private set; }
	public static int GcGen0CollectionCount { get; private set; }
	public static int GcGen1CollectionCount { get; private set; }
	public static int GcGen2CollectionCount { get; private set; }
	public static TimeSpan GcPauseDuration { get; private set; }
	public static bool Updated { get; private set; }

	private static long intervalStartTimestamp;
	private static int frameCount;
	private static long minimumFrameTimeTicks;
	private static long maximumFrameTimeTicks;
	private static long intervalStartAllocations;
	private static int gcGen0Collections;
	private static int gcGen1Collections;
	private static int gcGen2Collections;
	private static TimeSpan gcPause;

	internal static void Initialize()
	{
		Log.Info(LogCategory.Core, "Initializing Frame Statistics");
	}

	internal static void Update()
	{
		frameCount++;
		minimumFrameTimeTicks = Math.Min(minimumFrameTimeTicks, Time.RawDeltaTicks);
		maximumFrameTimeTicks = Math.Max(maximumFrameTimeTicks, Time.RawDeltaTicks);

		long intervalTicks = Time.FrameTimestamp - intervalStartTimestamp;
		Updated = intervalTicks >= Stopwatch.Frequency;

		if (!Updated)
		{
			return;
		}

		long allocations = GC.GetAllocatedBytesForCurrentThread();
		AllocatedBytes = allocations - intervalStartAllocations;
		intervalStartAllocations = allocations;

		int gen0Collections = GC.CollectionCount(0);
		GcGen0CollectionCount = gen0Collections - gcGen0Collections;
		gcGen0Collections = gen0Collections;

		int gen1Collections = GC.CollectionCount(1);
		GcGen1CollectionCount = gen1Collections - gcGen1Collections;
		gcGen1Collections = gen1Collections;

		int gen2Collections = GC.CollectionCount(2);
		GcGen2CollectionCount = gen2Collections - gcGen2Collections;
		gcGen2Collections = gen2Collections;

		var pause = GC.GetTotalPauseDuration();
		GcPauseDuration = pause - gcPause;
		gcPause = pause;

		double seconds = intervalTicks / (double)Stopwatch.Frequency;
		FramesPerSecond = frameCount / seconds;
		AverageFrameTime = seconds / frameCount;
		MinimumFrameTime = minimumFrameTimeTicks / (double)Stopwatch.Frequency;
		MaximumFrameTime = maximumFrameTimeTicks / (double)Stopwatch.Frequency;
		ResetInterval(Time.FrameTimestamp);
	}

	internal static void Start(long timestamp)
	{
		intervalStartAllocations = GC.GetAllocatedBytesForCurrentThread();
		gcGen0Collections = GC.CollectionCount(0);
		gcGen1Collections = GC.CollectionCount(1);
		gcGen2Collections = GC.CollectionCount(2);
		gcPause = GC.GetTotalPauseDuration();
		ResetInterval(timestamp);
	}

	private static void ResetInterval(long timestamp)
	{
		intervalStartTimestamp = timestamp;
		frameCount = 0;
		minimumFrameTimeTicks = long.MaxValue;
		maximumFrameTimeTicks = 0;
	}

	internal static void Dispose()
	{
		Log.Info(LogCategory.Core, "Disposing Frame Statistics");
	}
}
