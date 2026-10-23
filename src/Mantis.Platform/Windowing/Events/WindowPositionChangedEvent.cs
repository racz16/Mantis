using Mantis.Core.Numerics;

namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowPositionChangedEvent
{
	public required Window Window { get; init; }
	public required Vector2I Position { get; init; }
}

public delegate void WindowPositionChangedHandler(in WindowPositionChangedEvent e);
