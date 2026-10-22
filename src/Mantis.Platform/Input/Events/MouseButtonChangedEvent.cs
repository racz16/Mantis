using Mantis.Platform.Windowing;

namespace Mantis.Platform.Input.Events;

public readonly struct MouseButtonChangedEvent
{
	public required MouseButton Button { get; init; }
	public required bool IsDown { get; init; }
	public required int Clicks { get; init; }
	public required Window? Window { get; init; }
}

public delegate void MouseButtonChangedHandler(in MouseButtonChangedEvent e);
