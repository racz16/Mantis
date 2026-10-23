using Mantis.Core.Numerics;

namespace Mantis.Platform.Windowing;

public readonly struct WindowCreationDescriptor
{
	public required Vector2I Size { get; init; }
	public required string Title { get; init; }
	public bool Fullscreen { get; init; }
	public bool Hidden { get; init; }
	public bool Borderless { get; init; }
	public bool Resizable { get; init; }
	public WindowState State { get; init; }
	public bool Focus { get; init; }
	public bool NonFocusable { get; init; }
	public bool AlwaysOnTop { get; init; }
	public bool Transparent { get; init; }
	public bool HighPixelDensity { get; init; }
}
