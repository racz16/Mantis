namespace Mantis.Platform.Events;

public readonly ref struct LocaleChangedEvent
{
	public required ReadOnlySpan<string> Locales { get; init; }
}

public delegate void LocaleChangedHandler(in LocaleChangedEvent e);
