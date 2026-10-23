using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using Mantis.Core.Logging;
using Mantis.Platform.Input.Events;

using SDL3;

namespace Mantis.Platform.Input;

public static class Controllers
{
	public static event ControllerAddedHandler? Added;
	public static event ControllerRemovedHandler? Removed;
	public static event ControllerAxisChangedHandler? AxisChanged;
	public static event ControllerButtonChangedHandler? ButtonChanged;
	public static event ControllerHatChangedHandler? HatChanged;
	public static event ControllerBatteryChangedHandler? BatteryChanged;

	public static ReadOnlySpan<Controller> All => CollectionsMarshal.AsSpan(controllers);

	private static readonly List<Controller> controllers = [];

	internal static void Initialize()
	{
		Log.Info(LogCategory.Controller, "Initializing Controllers");
	}

	internal static void BeginFrame()
	{
		foreach (var controller in controllers)
		{
			controller.BeginFrame();
		}
	}

	internal static void OnAdded(in SDL.JoyDeviceEvent e)
	{
		var controller = new Controller(e.Which);
		controllers.Add(controller);
		Log.Debug(LogCategory.Controller, $"Controller [{controller.Name}] was added");
		var mantisEvent = new ControllerAddedEvent()
		{
			Controller = controller,
		};
		Added?.Invoke(in mantisEvent);
	}

	internal static void OnAdded(in SDL.GamepadDeviceEvent e)
	{
		if (TryGetController(e.Which, out var controller))
		{
			controller.Gamepad?.Close();
			controller.Gamepad = new Gamepad(e.Which);
			Log.Trace(LogCategory.Controller, $"Controller [{controller.Name}]'s gamepad mapping was added");
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller with ID {e.Which} whose gamepad mapping was added");
		}
	}

	internal static void OnRemoved(in SDL.JoyDeviceEvent e)
	{
		if (TryGetController(e.Which, out var controller))
		{
			controller.Close();
			_ = controllers.Remove(controller);
			Log.Debug(LogCategory.Controller, $"Controller [{controller.Name}] was removed");
			var mantisEvent = new ControllerRemovedEvent()
			{
				Controller = controller,
			};
			Removed?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller with ID {e.Which} which was removed");
		}
	}

	internal static void OnRemoved(in SDL.GamepadDeviceEvent e)
	{
		if (TryGetController(e.Which, out var controller))
		{
			controller.Gamepad?.Close();
			controller.Gamepad = null;
			Log.Trace(LogCategory.Controller, $"Controller [{controller.Name}]'s gamepad mapping was removed");
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller with ID {e.Which} whose gamepad mapping was removed");
		}
	}

	internal static void OnAxisChanged(in SDL.JoyAxisEvent e)
	{
		if (TryGetController(e.Which, out var controller))
		{
			controller.OnAxisChanged(in e);
			float value = controller.GetAxisValue(e.Axis);
			var mantisEvent = new ControllerAxisChangedEvent()
			{
				Axis = e.Axis,
				Value = value,
				Controller = controller,
			};
			AxisChanged?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller with ID {e.Which} whose axis changed");
		}
	}

	internal static void OnAxisChanged(in SDL.GamepadAxisEvent e)
	{
		if (TryGetController(e.Which, out var controller) && controller.Gamepad != null)
		{
			controller.Gamepad.OnAxisChanged(in e);
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller or gamepad with ID {e.Which} whose axis changed");
		}
	}

	internal static void OnButtonChanged(in SDL.JoyButtonEvent e)
	{
		if (TryGetController(e.Which, out var controller))
		{
			controller.OnButtonChanged(in e);
			var mantisEvent = new ControllerButtonChangedEvent()
			{
				Button = e.Button,
				IsDown = e.Down,
				Controller = controller,
			};
			ButtonChanged?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller with ID {e.Which} whose button changed");
		}
	}

	internal static void OnButtonChanged(in SDL.GamepadButtonEvent e)
	{
		if (TryGetController(e.Which, out var controller) && controller.Gamepad != null)
		{
			controller.Gamepad.OnButtonChanged(in e);
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller or gamepad with ID {e.Which} whose button changed");
		}
	}

	internal static void OnHatChanged(in SDL.JoyHatEvent e)
	{
		if (TryGetController(e.Which, out var controller))
		{
			controller.OnHatChanged(in e);
			var mantisEvent = new ControllerHatChangedEvent()
			{
				Hat = e.Hat,
				Direction = (ControllerHatDirection)e.Value,
				Controller = controller,
			};
			HatChanged?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller with ID {e.Which} whose hat changed");
		}
	}

	internal static void OnBatteryChanged(in SDL.JoyBatteryEvent e)
	{
		if (TryGetController(e.Which, out var controller))
		{
			controller.OnBatteryChanged(in e);
			Log.Trace(LogCategory.Controller, $"Controller [{controller.Name}]'s battery changed");
			var mantisEvent = new ControllerBatteryChangedEvent()
			{
				BatteryState = e.State.ToMantis(),
				BatteryPercent = e.Percent,
				Controller = controller,
			};
			BatteryChanged?.Invoke(in mantisEvent);
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller with ID {e.Which} whose battery changed");
		}
	}

	internal static void OnRemapped(in SDL.GamepadDeviceEvent e)
	{
		if (TryGetController(e.Which, out var controller) && controller.Gamepad != null)
		{
			controller.Gamepad.OnRemapped();
			Log.Trace(LogCategory.Controller, $"Controller [{controller.Name}]'s gamepad mapping was updated");
		}
		else
		{
			Log.Warn(LogCategory.Controller, $"Couldn't find controller or gamepad with ID {e.Which} which was remapped");
		}
	}

	internal static bool TryGetController(uint instanceId, [NotNullWhen(true)] out Controller? controller)
	{
		foreach (var candidate in controllers)
		{
			if (candidate.InstanceId == instanceId)
			{
				controller = candidate;
				return true;
			}
		}
		controller = null;
		return false;
	}

	internal static void Dispose()
	{
		Log.Info(LogCategory.Controller, "Disposing Controllers");

		foreach (var controller in controllers)
		{
			controller.Close();
		}
		controllers.Clear();
	}
}
