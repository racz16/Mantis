using System.Numerics;

namespace Mantis.Core;

public static class Utility
{
	[DebuggerHidden]
	public static void Breakpoint()
	{
		if (Debugger.IsAttached)
		{
			Debugger.Break();
		}
	}

	public static bool TryAppend(ReadOnlySpan<char> text, Span<char> destination, ref int written)
	{
		if (!text.TryCopyTo(destination[written..]))
		{
			return false;
		}
		written += text.Length;
		return true;
	}

	public static bool TryAppend<T>(T value, Span<char> destination, ref int written, ReadOnlySpan<char> format, IFormatProvider? provider) where T : ISpanFormattable
	{
		if (!value.TryFormat(destination[written..], out int count, format, provider))
		{
			return false;
		}
		written += count;
		return true;
	}

	public static bool IsBitSet(ReadOnlySpan<ulong> bits, int index)
	{
		return (bits[index >> 6] & (1ul << index)) != 0;
	}

	public static bool IsBitSet<T>(T bits, int index) where T : IBinaryInteger<T>
	{
		return (bits & (T.One << index)) != T.Zero;
	}

	public static void SetBit(Span<ulong> bits, int index)
	{
		bits[index >> 6] |= 1ul << index;
	}

	public static void SetBit<T>(ref T bits, int index) where T : IBinaryInteger<T>
	{
		bits |= T.One << index;
	}

	public static void ClearBit(Span<ulong> bits, int index)
	{
		bits[index >> 6] &= ~(1ul << index);
	}

	public static void ClearBit<T>(ref T bits, int index) where T : IBinaryInteger<T>
	{
		bits &= ~(T.One << index);
	}
}
