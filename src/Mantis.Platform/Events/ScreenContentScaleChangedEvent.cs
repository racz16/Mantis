namespace Mantis.Platform.Events;

public readonly struct ScreenContentScaleChangedEvent
{
	public required Screen Screen { get; init; }
}

public delegate void ScreenContentScaleChangedHandler(in ScreenContentScaleChangedEvent e);
