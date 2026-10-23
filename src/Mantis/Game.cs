using System.Diagnostics;

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
		// TODO: move it to time handling
		var sw = Stopwatch.StartNew();
		while (!shouldQuit)
		{
			//Log.Trace(LogCategory.Core, "Frame started");
			PlatformContext.PumpEvents();
			PlatformContext.DispatchEvents();
			MainThread.Drain();
			// TODO: move it to time handling
			float deltaTime = sw.ElapsedTicks * 1f / Stopwatch.Frequency;
			sw.Restart();

			Update(deltaTime);
		}
	}

	// TODO: move it to time handling
	protected virtual void Update(float deltaTime) { }

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

	private void DisposeThreading()
	{
		MainThread.Dispose();
	}

	protected virtual void UserDispose() { }
}
