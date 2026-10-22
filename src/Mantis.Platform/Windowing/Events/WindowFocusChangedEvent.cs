namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowFocusChangedEvent
{
	public required Window Window { get; init; }
	public required bool IsFocused { get; init; }
}

public delegate void WindowFocusChangedHandler(in WindowFocusChangedEvent e);
