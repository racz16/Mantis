namespace Mantis.Platform.Events;

public readonly struct ScreenUsableAreaChangedEvent
{
	public required Screen Screen { get; init; }
}

public delegate void ScreenUsableAreaChangedHandler(in ScreenUsableAreaChangedEvent e);
