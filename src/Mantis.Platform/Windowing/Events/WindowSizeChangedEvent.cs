using Mantis.Core.Numerics;

namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowSizeChangedEvent
{
	public required Window Window { get; init; }
	public required Vector2I Size { get; init; }
}

public delegate void WindowSizeChangedHandler(in WindowSizeChangedEvent e);
