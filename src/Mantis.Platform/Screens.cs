using Mantis.Core.Logging;
using Mantis.Platform.Events;

using SDL3;

namespace Mantis.Platform;

public static class Screens
{
	#region Events

	public static event ScreenOrientationChangedHandler? OrientationChanged;
	public static event ScreenAddedHandler? Added;
	public static event ScreenRemovedHandler? Removed;
	public static event ScreenPositionChangedHandler? PositionChanged;
	public static event ScreenModeChangedHandler? ModeChanged;
	public static event ScreenContentScaleChangedHandler? ContentScaleChanged;
	public static event ScreenUsableAreaChangedHandler? UsableAreaChanged;

	#endregion

	public static ReadOnlySpan<Screen> All => screens;
	public static Screen? Primary { get; private set; }

	private static Screen[] screens = [];

	internal static void Initialize()
	{
		Log.Info(LogCategory.Screen, "Initializing Screens");
		Update();
	}

	private static void Update()
	{
		Primary = null;

		uint primaryDisplayId = SDL.GetPrimaryDisplay();
		PlatformContext.ApiCall(primaryDisplayId, LogCategory.Screen);

		uint[]? displayIds = SDL.GetDisplays(out int displayCount);
		PlatformContext.ApiCallRef(displayIds, LogCategory.Screen);

		screens = new Screen[displayCount];

		if (displayIds != null)
		{
			for (int i = 0; i < displayIds.Length; i++)
			{
				AddScreen(displayIds, primaryDisplayId, i);
			}
		}
	}

	internal static bool TryGetScreen(uint id, out Screen screen)
	{
		foreach (ref readonly var candidate in All)
		{
			if (candidate.Id == id)
			{
				screen = candidate;
				return true;
			}
		}
		screen = default;
		return false;
	}

	private static void AddScreen(uint[] displayIds, uint primaryDisplayId, int i)
	{
		uint displayId = displayIds[i];
		string? name = SDL.GetDisplayName(displayId);
		PlatformContext.ApiCallRef(name, LogCategory.Screen);

		bool primary = displayId == primaryDisplayId;

		var orientation = SDL.GetCurrentDisplayOrientation(displayId);

		float contentScale = SDL.GetDisplayContentScale(displayId);
		PlatformContext.ApiCall(contentScale, LogCategory.Screen);

		bool result = SDL.GetDisplayBounds(displayId, out var displayBounds);
		PlatformContext.ApiCall(result, LogCategory.Screen);

		result = SDL.GetDisplayUsableBounds(displayId, out var usableBounds);
		PlatformContext.ApiCall(result, LogCategory.Screen);

		var displayMode = SDL.GetCurrentDisplayMode(displayId);
		PlatformContext.ApiCallRef(displayMode, LogCategory.Screen);
		float refreshRate = 0;
		float pixelDensity = 0;
		if (displayMode.HasValue)
		{
			refreshRate = displayMode.Value.RefreshRate;
			pixelDensity = displayMode.Value.PixelDensity;
		}

		var screen = new Screen()
		{
			Id = displayId,
			Name = name ?? "Unnamed screen",
			IsPrimary = primary,
			ContentScale = contentScale,
			RefreshRate = refreshRate,
			PixelDensity = pixelDensity,
			Size = new(displayBounds.W, displayBounds.H),
			Position = new(displayBounds.X, displayBounds.Y),
			UsableSize = new(usableBounds.W, usableBounds.H),
			UsablePosition = new(usableBounds.X, usableBounds.Y),
			Orientation = orientation.ToMantis(),
		};
		screens[i] = screen;
		if (primary)
		{
			Primary = screen;
		}
	}

	internal static void OnOrientationChanged(in SDL.DisplayEvent e)
	{
		Update();
		if (TryGetScreen(e.DisplayID, out var screen))
		{
			Log.Trace(LogCategory.Screen, $"Screen [{screen.Name}]'s orientation changed to {screen.Orientation}");
			var mantisEvent = new ScreenOrientationChangedEvent()
			{
				Screen = screen,
			};
			OrientationChanged?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Screen, $"Couldn't find screen with ID {e.DisplayID} whose orientation changed");
		}
	}

	internal static void OnAdded(in SDL.DisplayEvent e)
	{
		Update();
		if (TryGetScreen(e.DisplayID, out var screen))
		{
			Log.Debug(LogCategory.Screen, $"Screen [{screen.Name}] was added");
			var mantisEvent = new ScreenAddedEvent()
			{
				Screen = screen,
			};
			Added?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Screen, $"Couldn't find screen with ID {e.DisplayID} that was added");
		}
	}

	internal static void OnRemoved(in SDL.DisplayEvent e)
	{
		bool found = TryGetScreen(e.DisplayID, out var screen);
		Update();
		if (found)
		{
			Log.Debug(LogCategory.Screen, $"Screen [{screen.Name}] was removed");
			var mantisEvent = new ScreenRemovedEvent()
			{
				Screen = screen,
			};
			Removed?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Screen, $"Couldn't find screen with ID {e.DisplayID} that was removed");
		}
	}

	internal static void OnPositionChanged(in SDL.DisplayEvent e)
	{
		Update();
		if (TryGetScreen(e.DisplayID, out var screen))
		{
			Log.Trace(LogCategory.Screen, $"Screen [{screen.Name}]'s position changed to {screen.Position}");
			var mantisEvent = new ScreenPositionChangedEvent()
			{
				Screen = screen,
			};
			PositionChanged?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Screen, $"Couldn't find screen with ID {e.DisplayID} whose position changed");
		}
	}

	internal static void OnScreenModeChanged(in SDL.DisplayEvent e)
	{
		Update();
		if (TryGetScreen(e.DisplayID, out var screen))
		{
			Log.Trace(LogCategory.Screen, $"Screen [{screen.Name}]'s mode changed");
			var mantisEvent = new ScreenModeChangedEvent()
			{
				Screen = screen,
			};
			ModeChanged?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Screen, $"Couldn't find screen with ID {e.DisplayID} whose screen mode changed");
		}
	}

	internal static void OnContentScaleChanged(in SDL.DisplayEvent e)
	{
		Update();
		if (TryGetScreen(e.DisplayID, out var screen))
		{
			Log.Trace(LogCategory.Screen, $"Screen [{screen.Name}]'s content scale changed to {screen.ContentScale}");
			var mantisEvent = new ScreenContentScaleChangedEvent()
			{
				Screen = screen,
			};
			ContentScaleChanged?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Screen, $"Couldn't find screen with ID {e.DisplayID} whose content scale changed");
		}
	}

	internal static void OnUsableAreaChanged(in SDL.DisplayEvent e)
	{
		Update();
		if (TryGetScreen(e.DisplayID, out var screen))
		{
			Log.Trace(LogCategory.Screen, $"Screen [{screen.Name}]'s usable area changed to {screen.UsablePosition} and {screen.UsableSize}");
			var mantisEvent = new ScreenUsableAreaChangedEvent()
			{
				Screen = screen,
			};
			UsableAreaChanged?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Screen, $"Couldn't find screen with ID {e.DisplayID} whose usable area changed");
		}
	}

	internal static void Dispose()
	{
		Log.Info(LogCategory.Screen, "Disposing Screens");

		Primary = null;
		screens = [];
	}
}
