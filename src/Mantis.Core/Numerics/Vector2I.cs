using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Mantis.Core.Numerics;

[StructLayout(LayoutKind.Sequential)]
public struct Vector2I : IEquatable<Vector2I>, IFormattable, ISpanFormattable
{
	#region Static properties

	public static Vector2I Zero => default;
	public static Vector2I One => new(1);
	public static Vector2I UnitX => new(1, 0);
	public static Vector2I UnitY => new(0, 1);

	#endregion

	#region Fields

	public int X;
	public int Y;

	private const int ELEMENT_COUNT = 2;

	#endregion

	#region Properties

	public int this[int index]
	{
		readonly get
		{
			return index switch
			{
				0 => X,
				1 => Y,
				_ => throw new ArgumentOutOfRangeException(nameof(index), $"You can only index a Vector2I with 0, or 1, not with {index}"),
			};
		}

		set
		{
			switch (index)
			{
				case 0:
					X = value;
					break;
				case 1:
					Y = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(index), $"You can only index a Vector2I with 0, or 1, not with {index}");
			}
		}
	}

	#endregion

	#region Constructors

	public Vector2I(int value)
	{
		X = value;
		Y = value;
	}

	public Vector2I(int x, int y)
	{
		X = x;
		Y = y;
	}

	public Vector2I(ReadOnlySpan<int> values)
	{
		if (values.Length < ELEMENT_COUNT)
		{
			throw new ArgumentException($"{nameof(values)} must contain at least {ELEMENT_COUNT} elements, but it only contains {values.Length}", nameof(values));
		}
		this = Unsafe.ReadUnaligned<Vector2I>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
	}

	#endregion

	#region Static methods

	public static Vector2I Abs(Vector2I value)
	{
		return new(
			Math.Abs(value.X),
			Math.Abs(value.Y)
		);
	}

	public static Vector2I Add(Vector2I left, Vector2I right)
	{
		return left + right;
	}

	public static bool All(Vector2I vector, int value)
	{
		return vector.X == value
			&& vector.Y == value;
	}

	public static bool Any(Vector2I vector, int value)
	{
		return vector.X == value
			|| vector.Y == value;
	}

	public static Vector2I BitwiseAnd(Vector2I left, Vector2I right)
	{
		return left & right;
	}

	public static Vector2I BitwiseOr(Vector2I left, Vector2I right)
	{
		return left | right;
	}

	public static Vector2I Clamp(Vector2I value, Vector2I min, Vector2I max)
	{
		return new(
			Math.Clamp(value.X, min.X, max.X),
			Math.Clamp(value.Y, min.Y, max.Y)
		);
	}

	public static Vector2I CopySign(Vector2I value, Vector2I sign)
	{
		return new(
			sign.X >= 0 ? Math.Abs(value.X) : -Math.Abs(value.X),
			sign.Y >= 0 ? Math.Abs(value.Y) : -Math.Abs(value.Y)
		);
	}

	public static int Count(Vector2I vector, int value)
	{
		int count = 0;
		if (vector.X == value)
		{
			count++;
		}
		if (vector.Y == value)
		{
			count++;
		}
		return count;
	}

	public static Vector2I Create(int value)
	{
		return new(value);
	}

	public static Vector2I Create(int x, int y)
	{
		return new(x, y);
	}

	public static Vector2I Create(ReadOnlySpan<int> values)
	{
		return new(values);
	}

	public static Vector2I CreateScalar(int x)
	{
		return new(x, 0);
	}

	public static long Cross(Vector2I left, Vector2I right)
	{
		return (long)left.X * right.Y - (long)left.Y * right.X;
	}

	public static float Distance(Vector2I left, Vector2I right)
	{
		return (float)Math.Sqrt(DistanceSquared(left, right));
	}

	public static long DistanceSquared(Vector2I left, Vector2I right)
	{
		return (left - right).LengthSquared();
	}

	public static Vector2I Divide(Vector2I left, int right)
	{
		return left / right;
	}

	public static Vector2I Divide(Vector2I left, Vector2I right)
	{
		return left / right;
	}

	public static long Dot(Vector2I left, Vector2I right)
	{
		return (long)left.X * right.X
			 + (long)left.Y * right.Y;
	}

	public static bool EqualsAll(Vector2I left, Vector2I right)
	{
		return left == right;
	}

	public static bool EqualsAny(Vector2I left, Vector2I right)
	{
		return left.X == right.X
			|| left.Y == right.Y;
	}

	public static bool GreaterThanAll(Vector2I left, Vector2I right)
	{
		return left.X > right.X
			&& left.Y > right.Y;
	}

	public static bool GreaterThanAny(Vector2I left, Vector2I right)
	{
		return left.X > right.X
			|| left.Y > right.Y;
	}

	public static bool GreaterThanOrEqualAll(Vector2I left, Vector2I right)
	{
		return left.X >= right.X
			&& left.Y >= right.Y;
	}

	public static bool GreaterThanOrEqualAny(Vector2I left, Vector2I right)
	{
		return left.X >= right.X
			|| left.Y >= right.Y;
	}

	public static int IndexOf(Vector2I vector, int value)
	{
		if (vector.X == value)
		{
			return 0;
		}
		if (vector.Y == value)
		{
			return 1;
		}
		return -1;
	}

	public static int LastIndexOf(Vector2I vector, int value)
	{
		if (vector.Y == value)
		{
			return 1;
		}
		if (vector.X == value)
		{
			return 0;
		}
		return -1;
	}

	public static bool LessThanAll(Vector2I left, Vector2I right)
	{
		return left.X < right.X
			&& left.Y < right.Y;
	}

	public static bool LessThanAny(Vector2I left, Vector2I right)
	{
		return left.X < right.X
			|| left.Y < right.Y;
	}

	public static bool LessThanOrEqualAll(Vector2I left, Vector2I right)
	{
		return left.X <= right.X
			&& left.Y <= right.Y;
	}

	public static bool LessThanOrEqualAny(Vector2I left, Vector2I right)
	{
		return left.X <= right.X
			|| left.Y <= right.Y;
	}

	public static unsafe Vector2I Load(int* source)
	{
		return *(Vector2I*)source;
	}

	public static Vector2I LoadUnsafe(ref readonly int source, nuint elementOffset)
	{
		return Unsafe.As<int, Vector2I>(ref Unsafe.Add(ref Unsafe.AsRef(in source), (nint)elementOffset));
	}

	public static Vector2I LoadUnsafe(ref readonly int source)
	{
		return Unsafe.As<int, Vector2I>(ref Unsafe.AsRef(in source));
	}

	public static Vector2I Max(Vector2I value1, Vector2I value2)
	{
		return new(
			Math.Max(value1.X, value2.X),
			Math.Max(value1.Y, value2.Y)
		);
	}

	public static Vector2I MaxMagnitude(Vector2I value1, Vector2I value2)
	{
		return new(
			Math.Abs(value1.X) > Math.Abs(value2.X) ? value1.X : value2.X,
			Math.Abs(value1.Y) > Math.Abs(value2.Y) ? value1.Y : value2.Y
		);
	}

	public static Vector2I Min(Vector2I value1, Vector2I value2)
	{
		return new(
			Math.Min(value1.X, value2.X),
			Math.Min(value1.Y, value2.Y)
		);
	}

	public static Vector2I MinMagnitude(Vector2I value1, Vector2I value2)
	{
		return new(
			Math.Abs(value1.X) < Math.Abs(value2.X) ? value1.X : value2.X,
			Math.Abs(value1.Y) < Math.Abs(value2.Y) ? value1.Y : value2.Y
		);
	}

	public static Vector2I Multiply(int left, Vector2I right)
	{
		return left * right;
	}

	public static Vector2I Multiply(Vector2I left, int right)
	{
		return left * right;
	}

	public static Vector2I Multiply(Vector2I left, Vector2I right)
	{
		return left * right;
	}

	public static Vector2I Negate(Vector2I value)
	{
		return -value;
	}

	public static bool None(Vector2I vector, int value)
	{
		return vector.X != value
			&& vector.Y != value;
	}

	public static Vector2I OnesComplement(Vector2I value)
	{
		return ~value;
	}

	public static Vector2I Shuffle(Vector2I vector, byte xIndex, byte yIndex)
	{
		return new(
			vector[xIndex],
			vector[yIndex]
		);
	}

	public static Vector2I Subtract(Vector2I left, Vector2I right)
	{
		return left - right;
	}

	public static long Sum(Vector2I value)
	{
		return (long)value.X + value.Y;
	}

	public static Vector2I Xor(Vector2I left, Vector2I right)
	{
		return left ^ right;
	}

	public static Vector2I DivideAndRoundUp(Vector2I value, Vector2I divisor)
	{
		return new(
			(value.X + divisor.X - 1) / divisor.X,
			(value.Y + divisor.Y - 1) / divisor.Y
		);
	}

	public static Vector2I Sign(Vector2I value)
	{
		return new(
			Math.Sign(value.X),
			Math.Sign(value.Y)
		);
	}

	#endregion

	#region Methods

	public readonly float Length()
	{
		return (float)Math.Sqrt(LengthSquared());
	}

	public readonly long LengthSquared()
	{
		return Dot(this, this);
	}

	public readonly void CopyTo(int[] array)
	{
		if (array.Length < ELEMENT_COUNT)
		{
			throw new ArgumentException($"{nameof(array)} must contain at least {ELEMENT_COUNT} elements, but it only contains {array.Length}", nameof(array));
		}
		Unsafe.As<int, Vector2I>(ref array[0]) = this;
	}

	public readonly void CopyTo(int[] array, int index)
	{
		if ((uint)index >= (uint)array.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(index), $"{nameof(index)} (value: {index}) is outside of {nameof(array)} (size: {array.Length})");
		}

		if (array.Length - index < ELEMENT_COUNT)
		{
			throw new ArgumentException($"There is not enough space in {nameof(array)} (size: {array.Length}) at index {index} to store {ELEMENT_COUNT} ints", nameof(array));
		}

		Unsafe.As<int, Vector2I>(ref array[index]) = this;
	}

	public readonly void CopyTo(Span<int> destination)
	{
		if (destination.Length < ELEMENT_COUNT)
		{
			throw new ArgumentException($"{nameof(destination)} must contain at least {ELEMENT_COUNT} elements, but it only contains {destination.Length}", nameof(destination));
		}
		Unsafe.As<int, Vector2I>(ref MemoryMarshal.GetReference(destination)) = this;
	}

	public readonly bool TryCopyTo(Span<int> destination)
	{
		if (destination.Length < ELEMENT_COUNT)
		{
			return false;
		}

		Unsafe.As<int, Vector2I>(ref MemoryMarshal.GetReference(destination)) = this;
		return true;
	}

	public override readonly bool Equals([NotNullWhen(true)] object? obj)
	{
		return obj is Vector2I vector && Equals(vector);
	}

	public readonly bool Equals(Vector2I other)
	{
		return this == other;
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(X, Y);
	}

	[SkipLocalsInit]
	public readonly string ToString([StringSyntax("NumericFormat")] string? format, IFormatProvider? formatProvider)
	{
		string separator = NumberFormatInfo.GetInstance(formatProvider).NumberGroupSeparator;
		var handler = new DefaultInterpolatedStringHandler((ELEMENT_COUNT - 1) * separator.Length + 2 + (ELEMENT_COUNT - 1), ELEMENT_COUNT, formatProvider, stackalloc char[512]);
		handler.AppendLiteral("<");
		handler.AppendFormatted(X, format);
		handler.AppendLiteral(separator);
		handler.AppendLiteral(" ");
		handler.AppendFormatted(Y, format);
		handler.AppendLiteral(">");
		return handler.ToStringAndClear();
	}

	public readonly string ToString([StringSyntax("NumericFormat")] string? format)
	{
		return ToString(format, CultureInfo.CurrentCulture);
	}

	public override readonly string ToString()
	{
		return ToString("G", CultureInfo.CurrentCulture);
	}

	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		charsWritten = 0;
		string separator = NumberFormatInfo.GetInstance(provider).NumberGroupSeparator;
		int written = 0;

		if (!Utility.TryAppend("<", destination, ref written))
		{
			return false;
		}
		if (!Utility.TryAppend(X, destination, ref written, format, provider))
		{
			return false;
		}
		if (!Utility.TryAppend(separator, destination, ref written))
		{
			return false;
		}
		if (!Utility.TryAppend(" ", destination, ref written))
		{
			return false;
		}
		if (!Utility.TryAppend(Y, destination, ref written, format, provider))
		{
			return false;
		}
		if (!Utility.TryAppend(">", destination, ref written))
		{
			return false;
		}

		charsWritten = written;
		return true;
	}

	public readonly int MinAxisIndex()
	{
		return X <= Y ? 0 : 1;
	}

	public readonly int MaxAxisIndex()
	{
		return X >= Y ? 0 : 1;
	}

	public readonly void Deconstruct(out int x, out int y)
	{
		x = X;
		y = Y;
	}

	#endregion

	#region Operators

	public static Vector2I operator +(Vector2I value)
	{
		return value;
	}

	public static Vector2I operator -(Vector2I value)
	{
		return new(
			-value.X,
			-value.Y
		);
	}

	public static Vector2I operator +(Vector2I left, Vector2I right)
	{
		return new(
			left.X + right.X,
			left.Y + right.Y
		);
	}

	public static Vector2I operator -(Vector2I left, Vector2I right)
	{
		return new(
			left.X - right.X,
			left.Y - right.Y
		);
	}

	public static Vector2I operator *(int left, Vector2I right)
	{
		return new(
			left * right.X,
			left * right.Y
		);
	}

	public static Vector2I operator *(Vector2I left, int right)
	{
		return new(
			left.X * right,
			left.Y * right
		);
	}

	public static Vector2I operator *(Vector2I left, Vector2I right)
	{
		return new(
			left.X * right.X,
			left.Y * right.Y
		);
	}

	public static Vector2I operator /(Vector2I left, int right)
	{
		return new(
			left.X / right,
			left.Y / right
		);
	}

	public static Vector2I operator /(Vector2I left, Vector2I right)
	{
		return new(
			left.X / right.X,
			left.Y / right.Y
		);
	}

	public static Vector2I operator ~(Vector2I value)
	{
		return new(
			~value.X,
			~value.Y
		);
	}

	public static Vector2I operator &(Vector2I left, Vector2I right)
	{
		return new(
			left.X & right.X,
			left.Y & right.Y
		);
	}

	public static Vector2I operator |(Vector2I left, Vector2I right)
	{
		return new(
			left.X | right.X,
			left.Y | right.Y
		);
	}

	public static Vector2I operator ^(Vector2I left, Vector2I right)
	{
		return new(
			left.X ^ right.X,
			left.Y ^ right.Y
		);
	}

	public static Vector2I operator <<(Vector2I value, int shiftAmount)
	{
		return new(
			value.X << shiftAmount,
			value.Y << shiftAmount
		);
	}

	public static Vector2I operator >>(Vector2I value, int shiftAmount)
	{
		return new(
			value.X >> shiftAmount,
			value.Y >> shiftAmount
		);
	}

	public static Vector2I operator >>>(Vector2I value, int shiftAmount)
	{
		return new(
			value.X >>> shiftAmount,
			value.Y >>> shiftAmount
		);
	}

	public static bool operator ==(Vector2I left, Vector2I right)
	{
		return left.X == right.X
			&& left.Y == right.Y;
	}

	public static bool operator !=(Vector2I left, Vector2I right)
	{
		return left.X != right.X
			|| left.Y != right.Y;
	}

	public static implicit operator Vector2(Vector2I value)
	{
		return new(value.X, value.Y);
	}

	public static explicit operator Vector2I(Vector2 value)
	{
		return new((int)value.X, (int)value.Y);
	}

	#endregion
}
