using Mantis.Core.Numerics;

namespace Mantis.Platform;

public readonly struct Screen : IEquatable<Screen>
{
	internal uint Id { get; init; }
	public required string Name { get; init; }
	public required bool IsPrimary { get; init; }
	public required float ContentScale { get; init; }
	public required float RefreshRate { get; init; }
	public required float PixelDensity { get; init; }
	public required Vector2I Size { get; init; }
	public required Vector2I Position { get; init; }
	public required Vector2I UsableSize { get; init; }
	public required Vector2I UsablePosition { get; init; }
	public required ScreenOrientation Orientation { get; init; }

	public static bool operator ==(Screen left, Screen right)
	{
		return left.Id == right.Id;
	}

	public static bool operator !=(Screen left, Screen right)
	{
		return left.Id != right.Id;
	}

	public bool Equals(Screen other)
	{
		return Id == other.Id;
	}

	public override bool Equals(object? obj)
	{
		return obj is Screen screen && Equals(screen);
	}

	public override int GetHashCode()
	{
		return (int)Id;
	}

	public override string ToString()
	{
		return Name;
	}
}
