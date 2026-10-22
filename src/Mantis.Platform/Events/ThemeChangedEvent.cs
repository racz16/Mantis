namespace Mantis.Platform.Events;

public readonly struct ThemeChangedEvent
{
	public required Theme Theme { get; init; }
}

public delegate void ThemeChangedHandler(in ThemeChangedEvent e);
