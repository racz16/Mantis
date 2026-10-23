namespace Mantis.Platform.Events;

public readonly struct ScreenOrientationChangedEvent
{
	public required Screen Screen { get; init; }
}

public delegate void ScreenOrientationChangedHandler(in ScreenOrientationChangedEvent e);
