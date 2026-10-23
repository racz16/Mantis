namespace Mantis.Platform.Input.Events;

public readonly struct ControllerHatChangedEvent
{
	public required int Hat { get; init; }
	public required ControllerHatDirection Direction { get; init; }
	public required Controller Controller { get; init; }
}

public delegate void ControllerHatChangedHandler(in ControllerHatChangedEvent e);
