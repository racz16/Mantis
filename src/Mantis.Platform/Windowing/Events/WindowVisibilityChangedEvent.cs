namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowVisibilityChangedEvent
{
	public required Window Window { get; init; }
	public required bool IsVisible { get; init; }
}

public delegate void WindowVisibilityChangedHandler(in WindowVisibilityChangedEvent e);
