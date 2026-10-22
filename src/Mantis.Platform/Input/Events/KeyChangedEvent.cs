using Mantis.Platform.Windowing;

namespace Mantis.Platform.Input.Events;

public readonly struct KeyChangedEvent
{
	public required Key PhysicalKey { get; init; }
	public required Key LogicalKey { get; init; }
	public required bool IsDown { get; init; }
	public required bool IsRepeat { get; init; }
	public required KeyModifiers Modifiers { get; init; }
	public required Window? Window { get; init; }
}

public delegate void KeyChangedHandler(in KeyChangedEvent e);
