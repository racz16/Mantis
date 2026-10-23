namespace Mantis.Platform.Input.Events;

public readonly struct ControllerBatteryChangedEvent
{
	public required BatteryState BatteryState { get; init; }
	public required int BatteryPercent { get; init; }
	public required Controller Controller { get; init; }
}

public delegate void ControllerBatteryChangedHandler(in ControllerBatteryChangedEvent e);
