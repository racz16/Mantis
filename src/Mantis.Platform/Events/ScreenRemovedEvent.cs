namespace Mantis.Platform.Events;

public readonly struct ScreenRemovedEvent
{
	public required Screen Screen { get; init; }
}

public delegate void ScreenRemovedHandler(in ScreenRemovedEvent e);
