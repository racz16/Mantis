namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowCloseRequestedEvent
{
	public required Window Window { get; init; }
}

public delegate void WindowCloseRequestedHandler(in WindowCloseRequestedEvent e);
