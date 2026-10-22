namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowStateChangedEvent
{
	public required Window Window { get; init; }
	public required WindowState State { get; init; }
}

public delegate void WindowStateChangedHandler(in WindowStateChangedEvent e);
