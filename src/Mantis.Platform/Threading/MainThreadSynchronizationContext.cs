using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

using Mantis.Core;
using Mantis.Core.Logging;

namespace Mantis.Platform.Threading;

internal sealed class MainThreadSynchronizationContext : SynchronizationContext, IDisposable
{
	private readonly record struct WorkItem(SendOrPostCallback Callback, object? State);

	private volatile bool disposed;
	private readonly ConcurrentQueue<WorkItem> queue = new();

	public override void Post(SendOrPostCallback callback, object? state)
	{
		ObjectDisposedException.ThrowIf(disposed, this);

		queue.Enqueue(new WorkItem(callback, state));
	}

	public override void Send(SendOrPostCallback callback, object? state)
	{
		ObjectDisposedException.ThrowIf(disposed, this);

		if (MainThread.IsCurrent)
		{
			callback(state);
			return;
		}

		using var completed = new ManualResetEventSlim(false);
		ExceptionDispatchInfo? failure = null;

		queue.Enqueue(new WorkItem(_ =>
		{
			try
			{
				callback(state);
			}
			catch (Exception exception)
			{
				failure = ExceptionDispatchInfo.Capture(exception);
			}
			finally
			{
				completed.Set();
			}
		}, null));

		completed.Wait();
		failure?.Throw();
	}

	public override SynchronizationContext CreateCopy()
	{
		return this;
	}

	internal void Drain()
	{
		int count = queue.Count;
		for (int i = 0; i < count && queue.TryDequeue(out var work); i++)
		{
			try
			{
				work.Callback(work.State);
			}
			catch (Exception ex)
			{
				Log.Error(LogCategory.Platform, ex);
				Utility.Breakpoint();
			}
		}
	}

	public void Dispose()
	{
		disposed = true;
	}
}
