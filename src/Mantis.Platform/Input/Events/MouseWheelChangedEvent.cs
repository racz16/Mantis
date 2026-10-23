using System.Numerics;
using System.Runtime.InteropServices;

using Mantis.Core.Numerics;
using Mantis.Platform.Windowing;

namespace Mantis.Platform.Input.Events;

[StructLayout(LayoutKind.Sequential)]
public readonly struct MouseWheelChangedEvent
{
	public required Vector2 WheelDelta { get; init; }
	public required Vector2I IntegerWheelDelta { get; init; }
	public required bool DirectionFlipped { get; init; }
	public required Window? Window { get; init; }
}

public delegate void MouseWheelChangedHandler(in MouseWheelChangedEvent e);
