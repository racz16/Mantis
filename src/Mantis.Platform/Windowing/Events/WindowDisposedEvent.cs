namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowDisposedEvent
{
	public required Window Window { get; init; }
}

public delegate void WindowDisposedHandler(in WindowDisposedEvent e);
