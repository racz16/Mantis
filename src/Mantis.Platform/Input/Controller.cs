using System.Runtime.InteropServices;

using Mantis.Core;
using Mantis.Core.Logging;

using SDL3;

namespace Mantis.Platform.Input;

public sealed class Controller
{
	internal nint Handle { get; }
	internal uint InstanceId { get; }
	public string Name { get; }
	public ControllerType Type { get; }
	public string HardwareId { get; }
	public int AxisCount { get; }
	public int ButtonCount { get; }
	public int HatCount { get; }
	public bool Connected { get; private set; } = true;

	public BatteryState BatteryState { get; private set; }
	public int? BatteryPercent { get; private set; }

	public Gamepad? Gamepad { get; internal set; }

	private readonly float[] axes;
	private readonly ulong[] down;
	private readonly ulong[] pressed;
	private readonly ulong[] released;
	private readonly ControllerHatDirection[] hats;

	internal Controller(uint instanceId)
	{
		InstanceId = instanceId;

		Handle = SDL.OpenJoystick(instanceId);
		PlatformContext.ApiCall(Handle, LogCategory.Controller);

		string? name = SDL.GetJoystickName(Handle);
		PlatformContext.ApiCallRef(name, LogCategory.Controller);
		Name = name ?? "Unnamed controller";

		Type = (ControllerType)SDL.GetJoystickType(Handle);

		int axisCount = SDL.GetNumJoystickAxes(Handle);
		PlatformContext.ApiCall(axisCount, LogCategory.Controller, -1, false);
		AxisCount = Math.Max(axisCount, 0);
		axes = new float[AxisCount];
		for (int i = 0; i < AxisCount; i++)
		{
			if (SDL.GetJoystickAxisInitialState(Handle, i, out short state))
			{
				axes[i] = PlatformHelper.NormalizeAxisValue(state);
			}
		}

		int buttonCount = SDL.GetNumJoystickButtons(Handle);
		PlatformContext.ApiCall(buttonCount, LogCategory.Controller, -1, false);
		ButtonCount = Math.Max(buttonCount, 0);
		int ulongBitCount = 8 * sizeof(ulong);
		int bitfieldCount = (ButtonCount + ulongBitCount - 1) / ulongBitCount;
		down = new ulong[bitfieldCount];
		pressed = new ulong[bitfieldCount];
		released = new ulong[bitfieldCount];

		int hatCount = SDL.GetNumJoystickHats(Handle);
		PlatformContext.ApiCall(hatCount, LogCategory.Controller, -1, false);
		HatCount = Math.Max(hatCount, 0);
		hats = new ControllerHatDirection[HatCount];

		var powerState = SDL.GetJoystickPowerInfo(Handle, out int percent);
		PlatformContext.ApiCall((int)powerState, LogCategory.Controller, (int)SDL.PowerState.Error, false);
		BatteryState = powerState.ToMantis();
		BatteryPercent = percent == -1 ? null : percent;

		var guid = SDL.GetJoystickGUID(Handle);
		var bytes = MemoryMarshal.AsBytes(new ReadOnlySpan<SDL.GUID>(in guid));
		HardwareId = Convert.ToHexStringLower(bytes);
	}

	public float GetAxisValue(int index)
	{
		return axes[index];
	}

	public bool IsButtonDown(int index)
	{
		return Utility.IsBitSet(down, index);
	}

	public bool IsButtonPressed(int index)
	{
		return Utility.IsBitSet(pressed, index);
	}

	public bool IsAnyButtonPressed()
	{
		ulong result = 0;
		foreach (ulong pressedBitfield in pressed)
		{
			result |= pressedBitfield;
		}
		return result != 0;
	}

	public bool IsButtonReleased(int index)
	{
		return Utility.IsBitSet(released, index);
	}

	public ControllerHatDirection GetHatDirection(int index)
	{
		return hats[index];
	}

	internal void BeginFrame()
	{
		Array.Clear(pressed);
		Array.Clear(released);
		Gamepad?.BeginFrame();
	}

	internal void OnAxisChanged(in SDL.JoyAxisEvent e)
	{
		axes[e.Axis] = PlatformHelper.NormalizeAxisValue(e.Value);
	}

	internal void OnButtonChanged(in SDL.JoyButtonEvent e)
	{
		if (e.Down)
		{
			Utility.SetBit(down, e.Button);
			Utility.SetBit(pressed, e.Button);
		}
		else
		{
			Utility.ClearBit(down, e.Button);
			Utility.SetBit(released, e.Button);
		}
	}

	internal void OnHatChanged(in SDL.JoyHatEvent e)
	{
		hats[e.Hat] = (ControllerHatDirection)e.Value;
	}

	internal void OnBatteryChanged(in SDL.JoyBatteryEvent e)
	{
		BatteryState = e.State.ToMantis();
		BatteryPercent = e.Percent;
	}

	internal void Close()
	{
		Connected = false;
		Gamepad?.Close();
		for (int i = 0; i < AxisCount; i++)
		{
			axes[i] = SDL.GetJoystickAxisInitialState(Handle, i, out short state) ? PlatformHelper.NormalizeAxisValue(state) : 0;
		}
		Array.Clear(down);
		Array.Clear(pressed);
		Array.Clear(released);
		Array.Clear(hats);
		SDL.CloseJoystick(Handle);
	}
}
