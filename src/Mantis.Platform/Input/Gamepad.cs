using System.Numerics;

using Mantis.Core;
using Mantis.Core.Logging;

using SDL3;

namespace Mantis.Platform.Input;

public sealed class Gamepad
{
	public float StickInnerDeadzone { get; set => field = Math.Clamp(value, 0f, StickOuterDeadzone); } = 0.15f;
	public float StickOuterDeadzone { get; set => field = Math.Clamp(value, StickInnerDeadzone, 1f); } = 0.95f;
	public Vector2 LeftStick => ApplyRadialDeadzone(GetAxisValue(GamepadAxis.LeftX), -GetAxisValue(GamepadAxis.LeftY));
	public Vector2 RightStick => ApplyRadialDeadzone(GetAxisValue(GamepadAxis.RightX), -GetAxisValue(GamepadAxis.RightY));

	internal nint Handle { get; }
	public GamepadType Type { get; private set; }
	private readonly float[] axes = new float[6];
	private ulong down;
	private ulong pressed;
	private ulong released;

	internal Gamepad(uint instanceId)
	{
		Handle = SDL.OpenGamepad(instanceId);
		PlatformContext.ApiCall(Handle, LogCategory.Controller);

		Type = GetGamepadType();
	}

	public float GetAxisValue(GamepadAxis axis)
	{
		return axes[(int)axis];
	}

	public bool IsButtonDown(GamepadButton button)
	{
		return Utility.IsBitSet(down, (int)button);
	}

	public bool IsButtonPressed(GamepadButton button)
	{
		return Utility.IsBitSet(pressed, (int)button);
	}

	public bool IsAnyButtonPressed()
	{
		return pressed != 0;
	}

	public bool IsButtonReleased(GamepadButton button)
	{
		return Utility.IsBitSet(released, (int)button);
	}

	internal void BeginFrame()
	{
		pressed = 0;
		released = 0;
	}

	internal void OnAxisChanged(in SDL.GamepadAxisEvent e)
	{
		axes[e.Axis] = PlatformHelper.NormalizeAxisValue(e.Value);
	}

	internal void OnButtonChanged(in SDL.GamepadButtonEvent e)
	{
		if (e.Down)
		{
			Utility.SetBit(ref down, e.Button);
			Utility.SetBit(ref pressed, e.Button);
		}
		else
		{
			Utility.ClearBit(ref down, e.Button);
			Utility.SetBit(ref released, e.Button);
		}
	}

	internal void OnRemapped()
	{
		Type = GetGamepadType();
	}

	private GamepadType GetGamepadType()
	{
		return (GamepadType)SDL.GetGamepadType(Handle);
	}

	private Vector2 ApplyRadialDeadzone(float x, float y)
	{
		var value = new Vector2(x, y);
		float length = value.Length();
		if (length <= StickInnerDeadzone)
		{
			return Vector2.Zero;
		}
		float scaled = Math.Min((length - StickInnerDeadzone) / (StickOuterDeadzone - StickInnerDeadzone), 1f);
		return value * (scaled / length);
	}

	internal void Close()
	{
		Array.Clear(axes);
		down = 0;
		pressed = 0;
		released = 0;
		SDL.CloseGamepad(Handle);
	}
}
