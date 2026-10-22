using System.Numerics;

using Mantis.Core;
using Mantis.Core.Logging;
using Mantis.Core.Numerics;
using Mantis.Platform.Input.Events;
using Mantis.Platform.Windowing;

using SDL3;

namespace Mantis.Platform.Input;

public static class Mouse
{
	#region Events

	public static event MousePositionChangedHandler? PositionChanged;
	public static event MouseButtonChangedHandler? ButtonChanged;
	public static event MouseWheelChangedHandler? WheelChanged;

	#endregion

	#region Properties

	public static Vector2 Position { get; private set; }
	public static Vector2 PositionDelta { get; private set; }

	public static Vector2 WheelDelta { get; private set; }
	public static Vector2I IntegerWheelDelta { get; private set; }

	public static SystemCursor Cursor { get; private set; }
	private static readonly nint[] cursorIds = new nint[Enum.GetValues<SystemCursor>().Length];
	public static bool CursorVisible => SDL.CursorVisible();

	public static Window? Window { get; private set; }

	#endregion

	private static readonly int buttonCount = Enum.GetValues<MouseButton>().Length;

	private static byte down;
	private static byte pressed;
	private static byte released;

	internal static void Initialize()
	{
		Log.Info(LogCategory.Mouse, "Initializing Mouse");

		_ = SDL.GetMouseState(out float x, out float y);
		Position = new(x, y);
	}

	public static bool IsButtonDown(MouseButton button)
	{
		return Utility.IsBitSet(down, (int)button);
	}

	public static bool IsButtonPressed(MouseButton button)
	{
		return Utility.IsBitSet(pressed, (int)button);
	}

	public static bool IsAnyButtonPressed()
	{
		return pressed != 0;
	}

	public static bool IsButtonReleased(MouseButton button)
	{
		return Utility.IsBitSet(released, (int)button);
	}

	public static void SetCursorVisibility(bool visible)
	{
		bool result;
		if (visible)
		{
			result = SDL.ShowCursor();
			PlatformContext.ApiCall(result, LogCategory.Mouse);
		}
		else
		{
			result = SDL.HideCursor();
			PlatformContext.ApiCall(result, LogCategory.Mouse);
		}
		if (result)
		{
			Log.Trace(LogCategory.Mouse, $"Cursor visibility changed to {(visible ? "visible" : "hidden")}");
		}
	}

	public static void SetCursor(SystemCursor cursor)
	{
		if (Cursor == cursor)
		{
			return;
		}
		int index = (int)cursor;
		if (cursorIds[index] == 0)
		{
			nint newCursorId = SDL.CreateSystemCursor(cursor.ToSdl());
			PlatformContext.ApiCall(newCursorId, LogCategory.Mouse);
			if (newCursorId == 0)
			{
				return;
			}

			cursorIds[index] = newCursorId;
		}

		nint id = cursorIds[index];

		bool result = SDL.SetCursor(id);
		PlatformContext.ApiCall(result, LogCategory.Mouse);

		if (result)
		{
			Cursor = cursor;
			Log.Trace(LogCategory.Mouse, $"The cursor is changed to {cursor}");
		}
	}

	internal static void BeginFrame()
	{
		pressed = 0;
		released = 0;
		WheelDelta = new();
		IntegerWheelDelta = new();
		PositionDelta = new();
	}

	internal static void OnPositionChanged(in SDL.MouseMotionEvent e)
	{
		Position = new(e.X, e.Y);
		var delta = new Vector2(e.XRel, e.YRel);
		PositionDelta += delta;

		_ = PlatformContext.TryGetWindow(e.WindowID, out var window);
		Window = window;
		var mantisEvent = new MousePositionChangedEvent()
		{
			Position = Position,
			Delta = delta,
			Window = window,
		};
		PositionChanged?.Invoke(in mantisEvent);
	}

	internal static void OnButtonChanged(in SDL.MouseButtonEvent e)
	{
		int index = e.Button - 1;
		if ((uint)index >= buttonCount)
		{
			return;
		}

		if (e.Down)
		{
			Utility.SetBit(ref down, index);
			Utility.SetBit(ref pressed, index);
		}
		else
		{
			Utility.ClearBit(ref down, index);
			Utility.SetBit(ref released, index);
		}

		_ = PlatformContext.TryGetWindow(e.WindowID, out var window);
		Window = window;
		var mantisEvent = new MouseButtonChangedEvent()
		{
			Button = (MouseButton)index,
			IsDown = e.Down,
			Clicks = e.Clicks,
			Window = window,
		};
		ButtonChanged?.Invoke(in mantisEvent);
	}

	internal static void OnWheelChanged(in SDL.MouseWheelEvent e)
	{
		var wheelDelta = new Vector2(e.X, e.Y);
		WheelDelta += wheelDelta;
		var integerWheelDelta = new Vector2I(e.IntegerX, e.IntegerY);
		IntegerWheelDelta += integerWheelDelta;

		_ = PlatformContext.TryGetWindow(e.WindowID, out var window);
		Window = window;
		var mantisEvent = new MouseWheelChangedEvent()
		{
			WheelDelta = wheelDelta,
			IntegerWheelDelta = integerWheelDelta,
			DirectionFlipped = e.Direction == SDL.MouseWheelDirection.Flipped,
			Window = window,
		};
		WheelChanged?.Invoke(in mantisEvent);
	}

	internal static void Dispose()
	{
		Log.Info(LogCategory.Mouse, "Disposing Mouse");

		foreach (nint cursorId in cursorIds)
		{
			if (cursorId != 0)
			{
				SDL.DestroyCursor(cursorId);
			}
		}
		Array.Clear(cursorIds);
		Cursor = SystemCursor.Default;

		down = 0;
		Position = new();
		Window = null;
	}
}
