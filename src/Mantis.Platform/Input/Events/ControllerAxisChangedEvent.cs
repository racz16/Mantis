namespace Mantis.Platform.Input.Events;

public readonly struct ControllerAxisChangedEvent
{
	public required int Axis { get; init; }
	public required float Value { get; init; }
	public required Controller Controller { get; init; }
}

public delegate void ControllerAxisChangedHandler(in ControllerAxisChangedEvent e);
