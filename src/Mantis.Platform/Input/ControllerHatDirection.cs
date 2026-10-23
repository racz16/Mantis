namespace Mantis.Platform.Input;

[Flags]
public enum ControllerHatDirection : byte
{
	Centered = 0,
	Up = 1,
	Right = 2,
	Down = 4,
	Left = 8,
}
