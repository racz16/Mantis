using System.Diagnostics;

namespace Mantis.Platform.Threading;

public static class MainThread
{
	public static bool IsCurrent => Environment.CurrentManagedThreadId == threadId;

	private static bool initialized;
	private static int threadId;
	private static MainThreadSynchronizationContext context = null!;

	public static void Initialize()
	{
		Debug.Assert(!initialized);

		Thread.CurrentThread.Name = "Main Thread";
		threadId = Environment.CurrentManagedThreadId;
		context = new MainThreadSynchronizationContext();
		SynchronizationContext.SetSynchronizationContext(context);
		initialized = true;
	}

	public static void Post(Action action)
	{
		context.Post(static state => ((Action)state!)(), action);
	}

	public static Task PostAsync(Action action)
	{
		var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		Post(() =>
		{
			try
			{
				action();
				completion.SetResult();
			}
			catch (Exception exception)
			{
				completion.SetException(exception);
			}
		});
		return completion.Task;
	}

	public static void Drain()
	{
		context.Drain();
	}

	public static void Dispose()
	{
		Debug.Assert(initialized);

		context.Dispose();
		context.Drain();
		SynchronizationContext.SetSynchronizationContext(null);
		initialized = false;
	}
}
