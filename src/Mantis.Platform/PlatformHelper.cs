using Mantis.Platform.Input;
using Mantis.Platform.Windowing;

using SDL3;

namespace Mantis.Platform;

internal static class PlatformHelper
{
	internal const uint SCANCODE_MASK = 1u << 30;

	internal static SDL.ProgressState ToSdl(this WindowProgressState state)
	{
		return state switch
		{
			WindowProgressState.None => SDL.ProgressState.None,
			WindowProgressState.Indeterminate => SDL.ProgressState.Indeterminate,
			WindowProgressState.Normal => SDL.ProgressState.Normal,
			WindowProgressState.Paused => SDL.ProgressState.Paused,
			WindowProgressState.Error => SDL.ProgressState.Error,
			_ => SDL.ProgressState.None,
		};
	}

	internal static Theme ToMantis(this SDL.SystemTheme theme)
	{
		return theme switch
		{
			SDL.SystemTheme.Unknown => Theme.Unknown,
			SDL.SystemTheme.Light => Theme.Light,
			SDL.SystemTheme.Dark => Theme.Dark,
			_ => Theme.Unknown,
		};
	}

	internal static BatteryState ToMantis(this SDL.PowerState state)
	{
		return state switch
		{
			SDL.PowerState.Unknown => BatteryState.Unknown,
			SDL.PowerState.NoBattery => BatteryState.NoBattery,
			SDL.PowerState.OnBattery => BatteryState.OnBattery,
			SDL.PowerState.Charging => BatteryState.Charging,
			SDL.PowerState.Charged => BatteryState.Charged,
			_ => BatteryState.Unknown,
		};
	}

	internal static SDL.SystemCursor ToSdl(this SystemCursor cursor)
	{
		return cursor switch
		{
			SystemCursor.Default => SDL.SystemCursor.Default,
			SystemCursor.Text => SDL.SystemCursor.Text,
			SystemCursor.Wait => SDL.SystemCursor.Wait,
			SystemCursor.Crosshair => SDL.SystemCursor.Crosshair,
			SystemCursor.Progress => SDL.SystemCursor.Progress,
			SystemCursor.TopLeftAndBottomRightResize => SDL.SystemCursor.NWSEResize,
			SystemCursor.TopRightAndBottomLeftResize => SDL.SystemCursor.NESWResize,
			SystemCursor.RightAndLeftResize => SDL.SystemCursor.EWResize,
			SystemCursor.TopAndBottomResize => SDL.SystemCursor.NSResize,
			SystemCursor.Move => SDL.SystemCursor.Move,
			SystemCursor.NotAllowed => SDL.SystemCursor.NotAllowed,
			SystemCursor.Pointer => SDL.SystemCursor.Pointer,
			SystemCursor.TopLeftResize => SDL.SystemCursor.NWResize,
			SystemCursor.TopResize => SDL.SystemCursor.NResize,
			SystemCursor.TopRightResize => SDL.SystemCursor.NEResize,
			SystemCursor.RightResize => SDL.SystemCursor.EResize,
			SystemCursor.BottomRightResize => SDL.SystemCursor.SEResize,
			SystemCursor.BottomResize => SDL.SystemCursor.SResize,
			SystemCursor.BottomLeftResize => SDL.SystemCursor.SWResize,
			SystemCursor.LeftResize => SDL.SystemCursor.WResize,
			SystemCursor.ContextMenu => SDL.SystemCursor.ContextMenu,
			SystemCursor.Help => SDL.SystemCursor.Help,
			SystemCursor.Cell => SDL.SystemCursor.Cell,
			SystemCursor.VerticalText => SDL.SystemCursor.VerticalText,
			SystemCursor.Alias => SDL.SystemCursor.Alias,
			SystemCursor.Copy => SDL.SystemCursor.Copy,
			SystemCursor.NoDrop => SDL.SystemCursor.NoDrop,
			SystemCursor.Grab => SDL.SystemCursor.Grab,
			SystemCursor.Grabbing => SDL.SystemCursor.Grabbing,
			SystemCursor.ColumnResize => SDL.SystemCursor.ColResize,
			SystemCursor.RowResize => SDL.SystemCursor.RowResize,
			SystemCursor.AllScroll => SDL.SystemCursor.AllScroll,
			SystemCursor.ZoomIn => SDL.SystemCursor.ZoomIn,
			SystemCursor.ZoomOut => SDL.SystemCursor.ZoomOut,
			_ => SDL.SystemCursor.Default,
		};
	}

	internal static ScreenOrientation ToMantis(this SDL.DisplayOrientation state)
	{
		return state switch
		{
			SDL.DisplayOrientation.Unknown => ScreenOrientation.Unknown,
			SDL.DisplayOrientation.Landscape => ScreenOrientation.Landscape,
			SDL.DisplayOrientation.LandscapeFlipped => ScreenOrientation.LandscapeFlipped,
			SDL.DisplayOrientation.Portrait => ScreenOrientation.Portrait,
			SDL.DisplayOrientation.PortraitFlipped => ScreenOrientation.PortraitFlipped,
			_ => ScreenOrientation.Unknown,
		};
	}

	internal static KeyModifiers ToMantis(this SDL.Keymod modifiers)
	{
		var result = KeyModifiers.None;
		if (modifiers.HasFlag(SDL.Keymod.LShift) || modifiers.HasFlag(SDL.Keymod.RShift))
		{
			result |= KeyModifiers.Shift;
		}
		if (modifiers.HasFlag(SDL.Keymod.LCtrl) || modifiers.HasFlag(SDL.Keymod.RCtrl))
		{
			result |= KeyModifiers.Ctrl;
		}
		if (modifiers.HasFlag(SDL.Keymod.LAlt) || modifiers.HasFlag(SDL.Keymod.RAlt))
		{
			result |= KeyModifiers.Alt;
		}
		if (modifiers.HasFlag(SDL.Keymod.Mode))
		{
			result |= KeyModifiers.AltGr;
		}
		if (modifiers.HasFlag(SDL.Keymod.LGUI) || modifiers.HasFlag(SDL.Keymod.RGUI))
		{
			result |= KeyModifiers.Meta;
		}
		if (modifiers.HasFlag(SDL.Keymod.Caps))
		{
			result |= KeyModifiers.CapsLock;
		}
		if (modifiers.HasFlag(SDL.Keymod.Num))
		{
			result |= KeyModifiers.NumLock;
		}
		if (modifiers.HasFlag(SDL.Keymod.Scroll))
		{
			result |= KeyModifiers.ScrollLock;
		}
		return result;
	}

	internal static Key ToPhysicalKey(this SDL.Scancode scancode)
	{
		return (Key)scancode;
	}

	internal static Key ToLogicalKey(this SDL.Scancode scancode)
	{
		uint keycode = (uint)SDL.GetKeyFromScancode(scancode, SDL.Keymod.None, true);
		return keycode switch
		{
			>= 'a' and <= 'z' => (Key)((int)Key.A + (int)(keycode - 'a')),
			>= '1' and <= '9' => (Key)((int)Key.Digit1 + (int)(keycode - '1')),
			'0' => Key.Digit0,
			'-' => Key.Minus,
			'=' => Key.Equals,
			'[' => Key.LeftBracket,
			']' => Key.RightBracket,
			'\\' => Key.Backslash,
			';' => Key.Semicolon,
			'\'' => Key.Apostrophe,
			'`' => Key.Backtick,
			',' => Key.Comma,
			'.' => Key.Period,
			'/' => Key.Slash,
			' ' => Key.Space,
			_ when keycode < 0x20 || keycode == 0x7F || (keycode & SCANCODE_MASK) != 0 => (Key)scancode,
			_ => Key.Unknown,
		};
	}

	internal static string GetKeyName(Key physicalKey, string name)
	{
		return physicalKey switch
		{
			Key.Enter => "Enter",
			Key.CapsLock => "Caps Lock",
			Key.PrintScreen => "Print Screen",
			Key.ScrollLock => "Scroll Lock",
			Key.PageUp => "Page Up",
			Key.PageDown => "Page Down",
			Key.NumLock => "Num Lock",
			Key.VolumeUp => "Volume Up",
			Key.VolumeDown => "Volume Down",
			Key.MediaNextTrack => "Next Track",
			Key.MediaPreviousTrack => "Previous Track",
			Key.MediaStop => "Media Stop",
			Key.MediaPlayPause => "Media Play Pause",
			_ => name.IsWhiteSpace() ? physicalKey.ToString() : name,
		};
	}

	internal static string GetNameLower(this WindowState state)
	{
		return state switch
		{
			WindowState.Normal => "normal",
			WindowState.Minimized => "minimized",
			WindowState.Maximized => "maximized",
			_ => "unknown",
		};
	}

	internal static float NormalizeAxisValue(short value)
	{
		return Math.Max(value / (float)short.MaxValue, -1f);
	}
}
