using Mantis.Core.Numerics;

namespace Mantis.Platform.Windowing.Events;

public readonly struct WindowPixelSizeChangedEvent
{
	public required Window Window { get; init; }
	public required Vector2I Size { get; init; }
}

public delegate void WindowPixelSizeChangedHandler(in WindowPixelSizeChangedEvent e);
