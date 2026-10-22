namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowMouseHoverChangedEvent
{
	public required Window Window { get; init; }
	public required bool IsMouseOverWindow { get; init; }
}

public delegate void WindowMouseHoverChangedHandler(in WindowMouseHoverChangedEvent e);
