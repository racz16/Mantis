namespace Mantis.Platform.Events;

public readonly struct ScreenModeChangedEvent
{
	public required Screen Screen { get; init; }
}

public delegate void ScreenModeChangedHandler(in ScreenModeChangedEvent e);
