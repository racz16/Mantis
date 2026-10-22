namespace Mantis.Platform.Input.Events;

public readonly struct ControllerRemovedEvent
{
	public required Controller Controller { get; init; }
}

public delegate void ControllerRemovedHandler(in ControllerRemovedEvent e);
