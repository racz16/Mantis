using System.Runtime.InteropServices;

namespace Mantis.Platform;

[StructLayout(LayoutKind.Sequential)]
public readonly struct PowerState
{
	public required BatteryState BatteryState { get; init; }
	public int? BatterySeconds { get; init; }
	public int? BatteryPercent { get; init; }
}
