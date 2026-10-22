namespace Mantis.Platform.Input.Events;

public readonly struct ControllerButtonChangedEvent
{
	public required int Button { get; init; }
	public required bool IsDown { get; init; }
	public required Controller Controller { get; init; }
}

public delegate void ControllerButtonChangedHandler(in ControllerButtonChangedEvent e);
