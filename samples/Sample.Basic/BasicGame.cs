using System.Globalization;

using Mantis;
using Mantis.Core;
using Mantis.Core.Logging;
using Mantis.Core.Logging.Sinks;
using Mantis.Platform;
using Mantis.Platform.Input;
using Mantis.Platform.Windowing;

namespace Sample.Basic;

public class BasicGame : Game
{
	private readonly NumberFormatInfo titleNumberFormat = new()
	{
		NumberGroupSeparator = "\u2009",
		NumberDecimalSeparator = ".",
	};

	public static void Main()
	{
		var game = new BasicGame();
		game.Run();
	}

	protected override LogInitializationDescriptor CreateLogInitializationDescriptor()
	{
		return new LogInitializationDescriptor()
		{
			MaximumLevel = LogLevel.Trace,
			Sinks =
			[
				new ConsoleLogSink()
				{
					MaximumLevel = LogLevel.Trace,
				},
				new FileLogSink()
				{
					MaximumLevel = LogLevel.Trace,
				},
			],
		};
	}

	protected override PlatformContextInitializationDescriptor CreatePlatformContextInitializationDescriptor()
	{
		return new() { };
	}

	protected override WindowCreationDescriptor CreateWindowCreationDescriptor()
	{
		return new()
		{
			Title = "Basic Game",
			Size = new(1280, 720),
			Resizable = true,
			HighPixelDensity = true,
		};
	}

	protected override void UserInitialize() { }

	protected override void UserDispose() { }

	protected override void Update()
	{
		if (FrameStatistics.Updated)
		{
			MainWindow.Title = string.Create(titleNumberFormat,
				$"Mantis | {FrameStatistics.FramesPerSecond:N0} FPS | " +
				$"{FrameStatistics.AverageFrameTime * 1000:N3} ms " +
				$"(min {FrameStatistics.MinimumFrameTime * 1000:N3}, max {FrameStatistics.MaximumFrameTime * 1000:N3}) | " +
				$"{FrameStatistics.AllocatedBytes:N0} B/s | " +
				$"GC {FrameStatistics.GcGen0CollectionCount}-{FrameStatistics.GcGen1CollectionCount}-{FrameStatistics.GcGen2CollectionCount} {FrameStatistics.GcPauseDuration.TotalMilliseconds:N3} ms");
		}

		if (Keyboard.IsKeyPressed(Key.Escape))
		{
			RequestQuit("User pressed Esc");
		}
		foreach (ref readonly var controller in Controllers.All)
		{
			if (controller.Gamepad != null && controller.Gamepad.IsButtonDown(GamepadButton.Back))
			{
				RequestQuit("User pressed the back button on a controller");
			}
		}
	}
}
