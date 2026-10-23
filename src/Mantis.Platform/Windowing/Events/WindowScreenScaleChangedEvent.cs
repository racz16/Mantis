namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowScreenScaleChangedEvent
{
	public required Window Window { get; init; }
	public required float ScreenScale { get; init; }
}

public delegate void WindowScreenScaleChangedHandler(in WindowScreenScaleChangedEvent e);
