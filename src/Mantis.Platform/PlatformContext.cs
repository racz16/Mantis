using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

using Mantis.Core;
using Mantis.Core.Logging;
using Mantis.Core.Numerics;
using Mantis.Platform.Events;
using Mantis.Platform.Input;
using Mantis.Platform.Windowing;

using SDL3;

namespace Mantis.Platform;

public static class PlatformContext
{
	public static event QuitRequestedHandler? QuitRequested;
	public static event LocaleChangedHandler? LocaleChanged;
	public static event ThemeChangedHandler? ThemeChanged;

	public static Theme Theme => SDL.GetSystemTheme().ToMantis();
	private static string[] locales = [];
	public static ReadOnlySpan<string> Locales => locales;

	private static bool initialized;
	private static readonly Dictionary<uint, Window> windows = [];
	private static SDL.Event[] events = new SDL.Event[256];
	private static int frameEventCount;

	[DebuggerHidden]
	internal static void ApiCallThrow(bool result, LogCategory logCategory, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		if (!result)
		{
			HandleError(logCategory, file, member, line, true);
		}
	}

	[DebuggerHidden]
	internal static void ApiCall(bool result, LogCategory logCategory, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
	{
		if (!result)
		{
			HandleError(logCategory, file, member, line);
		}
	}

	[DebuggerHidden]
	internal static void ApiCallRef<T>(T? result, LogCategory logCategory, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : class
	{
		if (result == null)
		{
			HandleError(logCategory, file, member, line, false);
		}
	}

	[DebuggerHidden]
	internal static void ApiCallRef<T>(T? result, LogCategory logCategory, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : struct
	{
		if (!result.HasValue)
		{
			HandleError(logCategory, file, member, line, false);
		}
	}

	[DebuggerHidden]
	internal static void ApiCall<T>(T result, LogCategory logCategory, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : INumber<T>
	{
		ApiCall(result, logCategory, T.Zero, false, file, member, line);
	}

	[DebuggerHidden]
	internal static void ApiCallThrow<T>(T result, LogCategory logCategory, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : INumber<T>
	{
		ApiCall(result, logCategory, T.Zero, true, file, member, line);
	}

	[DebuggerHidden]
	internal static void ApiCall<T>(T id, LogCategory logCategory, T invalidValue, bool throwException, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0) where T : INumber<T>
	{
		if (id == invalidValue)
		{
			HandleError(logCategory, file, member, line, throwException);
		}
	}

	[DebuggerHidden]
	private static void HandleError(LogCategory logCategory, [CallerFilePath] string file = "", [CallerMemberName] string member = "", [CallerLineNumber] int line = 0, bool throwException = false)
	{
		string message = SDL.GetError();
		Log.Error(logCategory, message, file, member, line);
		Utility.Breakpoint();
		if (throwException)
		{
			throw new PlatformException(message);
		}
	}

	internal static void AddWindow(Window window)
	{
		if (!windows.TryAdd(window.Id, window))
		{
			Log.Warn(LogCategory.Window, $"Failed to add window [{window.Title}] with ID {window.Id} to the platform context");
		}
	}

	internal static void RemoveWindow(Window window)
	{
		bool result = windows.Remove(window.Id);
		if (!result)
		{
			Log.Warn(LogCategory.Window, $"Trying to remove window [{window.Title}] with ID {window.Id}, but couldn't find it");
		}
	}

	public static void Initialize(in PlatformContextInitializationDescriptor descriptor)
	{
		Log.Info(LogCategory.Platform, "Initializing the platform context");
		Debug.Assert(!initialized);

		bool result = SDL.Init(SDL.InitFlags.Video | SDL.InitFlags.Gamepad | SDL.InitFlags.Joystick);
		ApiCallThrow(result, LogCategory.Platform);

		try
		{
			DisableEvents();

			locales = GetLocales();
			Screens.Initialize();
			Keyboard.Initialize();
			Mouse.Initialize();
			Controllers.Initialize();

			initialized = true;
		}
		catch (Exception)
		{
			SDL.Quit();
			throw;
		}
	}

	private static void DisableEvents()
	{
		SDL.SetEventEnabled((uint)SDL.EventType.WindowMetalViewResized, false);
		SDL.SetEventEnabled((uint)SDL.EventType.WindowHitTest, false);
		SDL.SetEventEnabled((uint)SDL.EventType.WindowICCProfChanged, false);
		SDL.SetEventEnabled((uint)SDL.EventType.WindowSafeAreaChanged, false);
		SDL.SetEventEnabled((uint)SDL.EventType.WindowDestroyed, false);
		SDL.SetEventEnabled((uint)SDL.EventType.WindowHDRStateChanged, false);
		SDL.SetEventEnabled((uint)SDL.EventType.WindowSettingsChanged, false);

		SDL.SetEventEnabled((uint)SDL.EventType.MouseAdded, false);
		SDL.SetEventEnabled((uint)SDL.EventType.MouseRemoved, false);

		SDL.SetEventEnabled((uint)SDL.EventType.KeyboardAdded, false);
		SDL.SetEventEnabled((uint)SDL.EventType.KeyboardRemoved, false);

		SDL.SetEventEnabled((uint)SDL.EventType.JoystickBallMotion, false);
		SDL.SetEventEnabled((uint)SDL.EventType.JoystickUpdateComplete, false);

		SDL.SetEventEnabled((uint)SDL.EventType.GamepadTouchpadUp, false);
		SDL.SetEventEnabled((uint)SDL.EventType.GamepadTouchpadDown, false);
		SDL.SetEventEnabled((uint)SDL.EventType.GamepadTouchpadMotion, false);
		SDL.SetEventEnabled((uint)SDL.EventType.GamepadSensorUpdate, false);
		SDL.SetEventEnabled((uint)SDL.EventType.GamepadUpdateComplete, false);
		SDL.SetEventEnabled((uint)SDL.EventType.GamepadSteamHandleUpdated, false);
	}

	private static string[] GetLocales()
	{
		var sdlLocales = SDL.GetPreferredLocales(out int count);
		string[] result = new string[count];
		ApiCall(sdlLocales != null, LogCategory.Platform);
		if (sdlLocales != null)
		{
			for (int i = 0; i < count; i++)
			{
				var locale = sdlLocales[i];
				result[i] = locale.Country == null ? locale.Language : $"{locale.Language}-{locale.Country}";
			}
		}
		return result;
	}

	public static PowerState GetPowerState()
	{
		var state = SDL.GetPowerInfo(out int seconds, out int percent);
		ApiCall((int)state, LogCategory.Platform, (int)SDL.PowerState.Error, false);
		return new()
		{
			BatteryState = state.ToMantis(),
			BatterySeconds = seconds == -1 ? null : seconds,
			BatteryPercent = percent == -1 ? null : percent,
		};
	}

	internal static bool TryGetWindow(uint windowId, [NotNullWhen(true)] out Window? window)
	{
		return windows.TryGetValue(windowId, out window);
	}

	#region Events

	public static void PumpEvents()
	{
		//Log.Trace(LogCategory, "Pumping platform events");
		SDL.PumpEvents();

		frameEventCount = 0;
		int peepEventCount;
		do
		{
			if (frameEventCount == events.Length)
			{
				Array.Resize(ref events, events.Length * 2);
			}
			peepEventCount = SDL.PeepEvents(events.AsSpan(frameEventCount), events.Length - frameEventCount, SDL.EventAction.GetEvent, (uint)SDL.EventType.First, (uint)SDL.EventType.Last);
			if (peepEventCount < 0)
			{
				HandleError(LogCategory.Platform);
				break;
			}
			frameEventCount += peepEventCount;
		} while (frameEventCount == events.Length);
	}

	public static void DispatchEvents()
	{
		BeginFrame();

		foreach (ref readonly var sdlEvent in events.AsSpan(0, frameEventCount))
		{
			try
			{
				var eventType = (SDL.EventType)sdlEvent.Type;
				switch (eventType)
				{
					// System events
					case SDL.EventType.Quit:
						OnQuitRequest();
						break;
					case SDL.EventType.LocaleChanged:
						OnLocaleChanged();
						break;
					case SDL.EventType.SystemThemeChanged:
						OnThemeChanged();
						break;

					// Display events
					case SDL.EventType.DisplayOrientation:
						Screens.OnOrientationChanged(in sdlEvent.Display);
						break;
					case SDL.EventType.DisplayAdded:
						Screens.OnAdded(in sdlEvent.Display);
						break;
					case SDL.EventType.DisplayRemoved:
						Screens.OnRemoved(in sdlEvent.Display);
						break;
					case SDL.EventType.DisplayMoved:
						Screens.OnPositionChanged(in sdlEvent.Display);
						break;
					case SDL.EventType.DisplayDesktopModeChanged:
					case SDL.EventType.DisplayCurrentModeChanged:
						Screens.OnScreenModeChanged(in sdlEvent.Display);
						break;
					case SDL.EventType.DisplayContentScaleChanged:
						Screens.OnContentScaleChanged(in sdlEvent.Display);
						break;
					case SDL.EventType.UsableBoundsChanged:
						Screens.OnUsableAreaChanged(in sdlEvent.Display);
						break;

					// Window events
					case SDL.EventType.WindowShown:
						HandleWindowVisibilityChanged(in sdlEvent.Window, true);
						break;
					case SDL.EventType.WindowHidden:
						HandleWindowVisibilityChanged(in sdlEvent.Window, false);
						break;
					case SDL.EventType.WindowMoved:
						HandleWindowPositionChanged(in sdlEvent.Window);
						break;
					case SDL.EventType.WindowResized:
						HandleWindowSizeChanged(in sdlEvent.Window);
						break;
					case SDL.EventType.WindowPixelSizeChanged:
						HandleWindowPixelSizeChanged(in sdlEvent.Window);
						break;
					case SDL.EventType.WindowRestored:
						HandleWindowStateChanged(in sdlEvent.Window, WindowState.Normal);
						break;
					case SDL.EventType.WindowMinimized:
						HandleWindowStateChanged(in sdlEvent.Window, WindowState.Minimized);
						break;
					case SDL.EventType.WindowMaximized:
						HandleWindowStateChanged(in sdlEvent.Window, WindowState.Maximized);
						break;
					case SDL.EventType.WindowMouseEnter:
						HandleWindowMouseHoverChanged(in sdlEvent.Window, true);
						break;
					case SDL.EventType.WindowMouseLeave:
						HandleWindowMouseHoverChanged(in sdlEvent.Window, false);
						break;
					case SDL.EventType.WindowFocusGained:
						HandleWindowFocusChanged(in sdlEvent.Window, true);
						break;
					case SDL.EventType.WindowFocusLost:
						HandleWindowFocusChanged(in sdlEvent.Window, false);
						break;
					case SDL.EventType.WindowCloseRequested:
						HandleWindowCloseRequest(in sdlEvent.Window);
						break;
					case SDL.EventType.WindowDisplayChanged:
						HandleWindowScreenChanged(in sdlEvent.Window);
						break;
					case SDL.EventType.WindowDisplayScaleChanged:
						HandleWindowScreenScaleChanged(in sdlEvent.Window);
						break;
					case SDL.EventType.WindowEnterFullscreen:
						HandleWindowFullscreenChanged(in sdlEvent.Window, true);
						break;
					case SDL.EventType.WindowLeaveFullscreen:
						HandleWindowFullscreenChanged(in sdlEvent.Window, false);
						break;

					// Keyboard events
					case SDL.EventType.KeyDown:
					case SDL.EventType.KeyUp:
						Keyboard.OnKeyChanged(in sdlEvent.Key);
						break;
					case SDL.EventType.KeymapChanged:
						Keyboard.OnLayoutChanged();
						break;

					// Mouse events
					case SDL.EventType.MouseMotion:
						Mouse.OnPositionChanged(in sdlEvent.Motion);
						break;
					case SDL.EventType.MouseButtonDown:
					case SDL.EventType.MouseButtonUp:
						Mouse.OnButtonChanged(in sdlEvent.Button);
						break;
					case SDL.EventType.MouseWheel:
						Mouse.OnWheelChanged(in sdlEvent.Wheel);
						break;

					// Joystick events
					case SDL.EventType.JoystickAdded:
						Controllers.OnAdded(in sdlEvent.JDevice);
						break;
					case SDL.EventType.JoystickRemoved:
						Controllers.OnRemoved(in sdlEvent.JDevice);
						break;
					case SDL.EventType.JoystickAxisMotion:
						Controllers.OnAxisChanged(in sdlEvent.JAxis);
						break;
					case SDL.EventType.JoystickButtonUp:
					case SDL.EventType.JoystickButtonDown:
						Controllers.OnButtonChanged(in sdlEvent.JButton);
						break;
					case SDL.EventType.JoystickHatMotion:
						Controllers.OnHatChanged(in sdlEvent.JHat);
						break;
					case SDL.EventType.JoystickBatteryUpdated:
						Controllers.OnBatteryChanged(in sdlEvent.JBattery);
						break;

					// Gamepad events
					case SDL.EventType.GamepadAdded:
						Controllers.OnAdded(in sdlEvent.GDevice);
						break;
					case SDL.EventType.GamepadRemoved:
						Controllers.OnRemoved(in sdlEvent.GDevice);
						break;
					case SDL.EventType.GamepadAxisMotion:
						Controllers.OnAxisChanged(in sdlEvent.GAxis);
						break;
					case SDL.EventType.GamepadButtonUp:
					case SDL.EventType.GamepadButtonDown:
						Controllers.OnButtonChanged(in sdlEvent.GButton);
						break;
					case SDL.EventType.GamepadRemapped:
						Controllers.OnRemapped(in sdlEvent.GDevice);
						break;
				}
			}
			catch (Exception ex)
			{
				Log.Error(LogCategory.Platform, ex);
				Utility.Breakpoint();
			}
		}
	}

	private static void BeginFrame()
	{
		Keyboard.BeginFrame();
		Mouse.BeginFrame();
		Controllers.BeginFrame();
	}

	#region System events

	private static void OnQuitRequest()
	{
		Log.Trace(LogCategory.Platform, "Quit requested");
		var mantisEvent = new QuitRequestedEvent();
		QuitRequested?.Invoke(in mantisEvent);
	}

	private static void OnLocaleChanged()
	{
		Log.Trace(LogCategory.Platform, $"Locale changed");
		locales = GetLocales();
		var mantisEvent = new LocaleChangedEvent()
		{
			Locales = Locales,
		};
		LocaleChanged?.Invoke(in mantisEvent);
	}

	private static void OnThemeChanged()
	{
		Log.Trace(LogCategory.Platform, $"Theme changed");
		var mantisEvent = new ThemeChangedEvent()
		{
			Theme = Theme,
		};
		ThemeChanged?.Invoke(in mantisEvent);
	}

	#endregion

	#region Window events

	private static void HandleWindowVisibilityChanged(in SDL.WindowEvent e, bool isVisible)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			Log.Trace(LogCategory.Window, $"Window [{window.Title}] became {(isVisible ? "visible" : "hidden")}");
			window.OnVisibilityChanged(isVisible);
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose visibility changed");
		}
	}

	private static void HandleWindowPositionChanged(in SDL.WindowEvent e)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			var position = new Vector2I(e.Data1, e.Data2);
			Log.Trace(LogCategory.Window, $"Window [{window.Title}] position changed to {position}");
			window.OnPositionChanged(position);
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose position changed");
		}
	}

	private static void HandleWindowSizeChanged(in SDL.WindowEvent e)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			var size = new Vector2I(e.Data1, e.Data2);
			Log.Trace(LogCategory.Window, $"Window [{window.Title}] size changed to {size}");
			window.OnSizeChanged(size);
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose size changed");
		}
	}

	private static void HandleWindowPixelSizeChanged(in SDL.WindowEvent e)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			var size = new Vector2I(e.Data1, e.Data2);
			Log.Trace(LogCategory.Window, $"Window [{window.Title}] pixel size changed to {size}");
			window.OnPixelSizeChanged(size);
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose pixel size changed");
		}
	}

	private static void HandleWindowStateChanged(in SDL.WindowEvent e, WindowState state)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			Log.Trace(LogCategory.Window, $"Window [{window.Title}] state changed to {state.GetNameLower()}");
			window.OnStateChanged(state);
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose state changed");
		}
	}

	private static void HandleWindowMouseHoverChanged(in SDL.WindowEvent e, bool isMouseOverWindow)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			Log.Trace(LogCategory.Window, $"The mouse {(isMouseOverWindow ? "is" : "isn't")} over Window [{window.Title}]");
			window.OnMouseHoverChanged(isMouseOverWindow);
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose mouse hover changed");
		}
	}

	private static void HandleWindowFocusChanged(in SDL.WindowEvent e, bool isFocused)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			Log.Trace(LogCategory.Window, $"Window [{window.Title}] focus {(isFocused ? "gained" : "lost")}");
			window.OnFocusChanged(isFocused);
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose focus changed");
		}
	}

	private static void HandleWindowCloseRequest(in SDL.WindowEvent e)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			Log.Trace(LogCategory.Window, $"Window [{window.Title}] was requested to close");
			window.OnCloseRequested();
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} that was requested to close");
		}
	}

	private static void HandleWindowScreenChanged(in SDL.WindowEvent e)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			if (Screens.TryGetScreen((uint)e.Data1, out var screen))
			{
				Log.Trace(LogCategory.Window, $"Window [{window.Title}]'s screen changed");
				window.OnScreenChanged(in screen);
			}
			else
			{
				Log.Warn(LogCategory.Window, $"Couldn't find screen with ID {e.Data1} where the window with ID {e.WindowID} was moved");
			}
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose screen changed");
		}
	}

	private static void HandleWindowScreenScaleChanged(in SDL.WindowEvent e)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			Log.Trace(LogCategory.Window, $"Window [{window.Title}]'s screen scale changed");
			window.OnScreenScaleChanged();
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose screen's scale was changed");
		}
	}

	private static void HandleWindowFullscreenChanged(in SDL.WindowEvent e, bool isInFullscreen)
	{
		if (windows.TryGetValue(e.WindowID, out var window))
		{
			Log.Trace(LogCategory.Window, $"Window [{window.Title}] {(isInFullscreen ? "entered" : "left")} fullscreen");
			window.OnFullscreenChanged(isInFullscreen);
		}
		else
		{
			Log.Warn(LogCategory.Window, $"Couldn't find window with ID {e.WindowID} whose fullscreen changed");
		}
	}

	#endregion

	#endregion

	public static void Dispose()
	{
		Log.Info(LogCategory.Platform, "Disposing the platform context");
		Debug.Assert(initialized);

		var currentWindows = windows.Values.ToList();
		foreach (var window in currentWindows)
		{
			window.Dispose();
		}
		windows.Clear();

		Controllers.Dispose();
		Mouse.Dispose();
		Keyboard.Dispose();
		Screens.Dispose();

		SDL.Quit();

		initialized = false;
	}
}
