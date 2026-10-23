using System.Numerics;
using System.Runtime.InteropServices;

using Mantis.Platform.Windowing;

namespace Mantis.Platform.Input.Events;

[StructLayout(LayoutKind.Sequential)]
public readonly struct MousePositionChangedEvent
{
	public required Vector2 Position { get; init; }
	public required Vector2 Delta { get; init; }
	public required Window? Window { get; init; }
}

public delegate void MousePositionChangedHandler(in MousePositionChangedEvent e);
