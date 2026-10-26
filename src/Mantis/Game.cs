using Mantis.Core;
using Mantis.Core.Logging;
using Mantis.Platform;
using Mantis.Platform.Events;
using Mantis.Platform.Threading;
using Mantis.Platform.Windowing;
using Mantis.Platform.Windowing.Events;

namespace Mantis;

public abstract class Game
{
	public Window MainWindow { get; private set; } = null!;

	private readonly record struct Subsystem(string Name, Action Initialize, Action Dispose);

	private bool shouldQuit;
	private Subsystem[] subsystems = [];
	private int initializedSubsystemCount;

	public void Run()
	{
		try
		{
			subsystems =
			[
				new("Threading", InitializeThreading, DisposeThreading),
				new("Logging", InitializeLogging, DisposeLogging),
				new("Time", InitializeTime, DisposeTime),
				new("Frame statistics", InitializeFrameStatistics, DisposeFrameStatistics),
				new("Platform", InitializePlatform, DisposePlatform),
				new("Main window", InitializeMainWindow, DisposeMainWindow),
				new("User", UserInitialize, UserDispose),
			];
			InitializeSubsystems();

			MainLoop();
		}
		catch (Exception ex)
		{
			Log.Fatal(LogCategory.Core, $"Unhandled exception: {ex}");
			throw;
		}
		finally
		{
			DisposeSubsystems();
		}
	}

	private void InitializeSubsystems()
	{
		for (int i = 0; i < subsystems.Length; i++)
		{
			subsystems[i].Initialize();
			initializedSubsystemCount++;
		}
	}

	private void InitializeThreading()
	{
		MainThread.Initialize();
	}

	private void InitializeLogging()
	{
		var descriptor = CreateLogInitializationDescriptor();
		Log.Initialize(in descriptor);
	}

	protected virtual LogInitializationDescriptor CreateLogInitializationDescriptor()
	{
		return new();
	}

	private void InitializeTime()
	{
		Time.Initialize();
	}

	private void InitializeFrameStatistics()
	{
		FrameStatistics.Initialize();
	}

	private void InitializePlatform()
	{
		var descriptor = CreatePlatformContextInitializationDescriptor();
		PlatformContext.Initialize(in descriptor);
		PlatformContext.QuitRequested += QuitRequestedEventHandler;
	}

	protected virtual PlatformContextInitializationDescriptor CreatePlatformContextInitializationDescriptor()
	{
		return new();
	}

	protected virtual void QuitRequestedEventHandler(in QuitRequestedEvent e)
	{
		RequestQuit("Quit requested");
	}

	private void InitializeMainWindow()
	{
		var descriptor = CreateWindowCreationDescriptor();
		MainWindow = new Window(in descriptor);
		MainWindow.CloseRequested += MainWindowCloseRequestedEventHandler;
	}

	protected virtual void MainWindowCloseRequestedEventHandler(in WindowCloseRequestedEvent e)
	{
		RequestQuit($"Window [{e.Window.Title}] closed");
	}

	protected void RequestQuit(string reason)
	{
		Log.Info(LogCategory.Core, $"Quit requested: {reason}");
		shouldQuit = true;
	}

	protected virtual WindowCreationDescriptor CreateWindowCreationDescriptor()
	{
		return new()
		{
			Size = new(1280, 720),
			Title = MantisEngine.NAME,
			Resizable = true,
			HighPixelDensity = true,
		};
	}

	protected virtual void UserInitialize() { }

	private void MainLoop()
	{
		Time.Reset();
		FrameStatistics.Start(Time.FrameTimestamp);
		while (!shouldQuit)
		{
			//Log.Trace(LogCategory.Core, "Frame started");
			Time.BeginFrame();
			FrameStatistics.Update();
			PlatformContext.PumpEvents();
			PlatformContext.DispatchEvents();
			MainThread.Drain();

			Update();
		}
	}

	protected virtual void Update() { }

	private void DisposeSubsystems()
	{
		for (int i = initializedSubsystemCount - 1; i >= 0; i--)
		{
			try
			{
				subsystems[i].Dispose();
			}
			catch (Exception ex)
			{
				Log.Error(LogCategory.Core, $"Error while disposing the {subsystems[i].Name} subsystem: {ex}");
			}
		}
	}

	private void DisposeMainWindow()
	{
		MainWindow.CloseRequested -= MainWindowCloseRequestedEventHandler;
		MainWindow.Dispose();
	}

	private void DisposePlatform()
	{
		PlatformContext.QuitRequested -= QuitRequestedEventHandler;
		PlatformContext.Dispose();
	}

	private static void DisposeLogging()
	{
		Log.Dispose();
	}

	private static void DisposeFrameStatistics()
	{
		FrameStatistics.Dispose();
	}

	private static void DisposeTime()
	{
		Time.Dispose();
	}

	private void DisposeThreading()
	{
		MainThread.Dispose();
	}

	protected virtual void UserDispose() { }
}
