namespace Mantis.Platform.Events;

public readonly struct ScreenAddedEvent
{
	public required Screen Screen { get; init; }
}

public delegate void ScreenAddedHandler(in ScreenAddedEvent e);
