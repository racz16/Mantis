namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowScreenChangedEvent
{
	public required Window Window { get; init; }
	public required Screen Screen { get; init; }
}

public delegate void WindowScreenChangedHandler(in WindowScreenChangedEvent e);
