namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowFullscreenChangedEvent
{
	public required Window Window { get; init; }
	public required bool IsInFullscreen { get; init; }
}

public delegate void WindowFullscreenChangedHandler(in WindowFullscreenChangedEvent e);
