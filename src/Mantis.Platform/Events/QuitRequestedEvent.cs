namespace Mantis.Platform.Events;

public readonly struct QuitRequestedEvent { }

public delegate void QuitRequestedHandler(in QuitRequestedEvent e);
