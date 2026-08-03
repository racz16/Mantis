using System.Collections.Concurrent;

namespace Mantis.Core.Logging;

public readonly struct LogCategory : IEquatable<LogCategory>
{
	private static int nextId;
	private static readonly ConcurrentDictionary<string, LogCategory> registry = new(StringComparer.Ordinal);

	public static LogCategory Core { get; } = Register("Core");

	public readonly int Id { get; }
	private readonly string Name { get; }

	public static LogCategory Register(string name)
	{
		return registry.GetOrAdd(name, static n => new LogCategory(Interlocked.Increment(ref nextId), n));
	}

	public static bool operator ==(LogCategory left, LogCategory right)
	{
		return left.Id == right.Id;
	}

	public static bool operator !=(LogCategory left, LogCategory right)
	{
		return left.Id != right.Id;
	}

	private LogCategory(int id, string name)
	{
		Id = id;
		Name = name;
	}

	public bool Equals(LogCategory other)
	{
		return Id == other.Id;
	}

	public override bool Equals(object? obj)
	{
		return obj is LogCategory category && Equals(category);
	}

	public override int GetHashCode()
	{
		return Id;
	}

	public override string ToString()
	{
		return Name ?? "Default";
	}
}
