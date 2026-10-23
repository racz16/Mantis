using Mantis.Core;
using Mantis.Core.Logging;
using Mantis.Platform.Input.Events;

using SDL3;

namespace Mantis.Platform.Input;

public static class Keyboard
{
	#region Events

	public static event KeyChangedHandler? KeyChanged;
	public static event KeyboardLayoutChangedHandler? LayoutChanged;

	#endregion

	private const int KEY_COUNT = 512;
	private const int ULONG_BIT_COUNT = 8 * sizeof(ulong);
	private const int BITFIELD_COUNT = KEY_COUNT / ULONG_BIT_COUNT;

	private static readonly ulong[] down = new ulong[BITFIELD_COUNT];
	private static readonly ulong[] pressed = new ulong[BITFIELD_COUNT];
	private static readonly ulong[] released = new ulong[BITFIELD_COUNT];

	internal static void Initialize()
	{
		Log.Info(LogCategory.Keyboard, "Initializing Keyboard");
	}

	public static bool IsKeyDown(Key key)
	{
		return Utility.IsBitSet(down, (int)key);
	}

	public static bool IsKeyPressed(Key key)
	{
		return Utility.IsBitSet(pressed, (int)key);
	}

	public static bool IsAnyKeyPressed()
	{
		ulong result = 0;
		foreach (ulong pressedBitfield in pressed)
		{
			result |= pressedBitfield;
		}
		return result != 0;
	}

	public static bool IsKeyReleased(Key key)
	{
		return Utility.IsBitSet(released, (int)key);
	}

	public static string GetKeyDisplayName(Key physicalKey)
	{
		var keycode = SDL.GetKeyFromScancode((SDL.Scancode)physicalKey, SDL.Keymod.None, false);
		string sdlName = SDL.GetKeyName(keycode);
		return PlatformHelper.GetKeyName(physicalKey, sdlName);
	}

	internal static void BeginFrame()
	{
		Array.Clear(pressed);
		Array.Clear(released);
	}

	internal static void OnKeyChanged(in SDL.KeyboardEvent e)
	{
		if (e.Scancode == SDL.Scancode.Unknown)
		{
			return;
		}
		int index = (int)e.Scancode;
		if (e.Down)
		{
			if (!e.Repeat)
			{
				Utility.SetBit(down, index);
				Utility.SetBit(pressed, index);
			}
		}
		else
		{
			Utility.ClearBit(down, index);
			Utility.SetBit(released, index);
		}

		_ = PlatformContext.TryGetWindow(e.WindowID, out var window);

		var mantisEvent = new KeyChangedEvent()
		{
			IsDown = e.Down,
			IsRepeat = e.Repeat,
			PhysicalKey = e.Scancode.ToPhysicalKey(),
			LogicalKey = e.Scancode.ToLogicalKey(),
			Modifiers = e.Mod.ToMantis(),
			Window = window,
		};
		KeyChanged?.Invoke(in mantisEvent);
	}

	internal static void OnLayoutChanged()
	{
		Log.Trace(LogCategory.Keyboard, $"The keyboard's layout changed");
		var mantisEvent = new KeyboardLayoutChangedEvent();
		LayoutChanged?.Invoke(in mantisEvent);
	}

	internal static void Dispose()
	{
		Log.Info(LogCategory.Keyboard, "Disposing Keyboard");

		Array.Clear(down);
	}
}
