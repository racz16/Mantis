namespace Mantis.Platform.Input;

[Flags]
public enum KeyModifiers : byte
{
	None = 0,
	Shift = 1,
	Ctrl = 2,
	Alt = 4,
	AltGr = 8,
	Meta = 16,
	NumLock = 32,
	CapsLock = 64,
	ScrollLock = 128,
}
