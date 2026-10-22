namespace Mantis.Platform.Input.Events;

public readonly struct ControllerAddedEvent
{
	public required Controller Controller { get; init; }
}

public delegate void ControllerAddedHandler(in ControllerAddedEvent e);
