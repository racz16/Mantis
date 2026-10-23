namespace Mantis.Platform.Input.Events;

public readonly struct KeyboardLayoutChangedEvent { }

public delegate void KeyboardLayoutChangedHandler(in KeyboardLayoutChangedEvent e);
