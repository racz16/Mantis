namespace Mantis.Platform.Events;

public readonly struct ScreenPositionChangedEvent
{
	public required Screen Screen { get; init; }
}

public delegate void ScreenPositionChangedHandler(in ScreenPositionChangedEvent e);
