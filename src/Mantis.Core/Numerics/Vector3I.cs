using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Mantis.Core.Numerics;

[StructLayout(LayoutKind.Sequential)]
public struct Vector3I : IEquatable<Vector3I>, IFormattable, ISpanFormattable
{
	#region Static properties

	public static Vector3I Zero => default;
	public static Vector3I One => new(1);
	public static Vector3I UnitX => new(1, 0, 0);
	public static Vector3I UnitY => new(0, 1, 0);
	public static Vector3I UnitZ => new(0, 0, 1);

	#endregion

	#region Fields

	public int X;
	public int Y;
	public int Z;

	private const int ELEMENT_COUNT = 3;

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
				2 => Z,
				_ => throw new ArgumentOutOfRangeException(nameof(index), $"You can only index a Vector3I with 0, 1, or 2, not with {index}"),
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
				case 2:
					Z = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(index), $"You can only index a Vector3I with 0, 1, or 2, not with {index}");
			}
		}
	}

	#endregion

	#region Constructors

	public Vector3I(int value)
	{
		X = value;
		Y = value;
		Z = value;
	}

	public Vector3I(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public Vector3I(ReadOnlySpan<int> values)
	{
		if (values.Length < ELEMENT_COUNT)
		{
			throw new ArgumentException($"{nameof(values)} must contain at least {ELEMENT_COUNT} elements, but it only contains {values.Length}", nameof(values));
		}
		this = Unsafe.ReadUnaligned<Vector3I>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
	}

	public Vector3I(Vector2I value, int z)
	{
		X = value.X;
		Y = value.Y;
		Z = z;
	}

	#endregion

	#region Static methods

	public static Vector3I Abs(Vector3I value)
	{
		return new(
			Math.Abs(value.X),
			Math.Abs(value.Y),
			Math.Abs(value.Z)
		);
	}

	public static Vector3I Add(Vector3I left, Vector3I right)
	{
		return left + right;
	}

	public static bool All(Vector3I vector, int value)
	{
		return vector.X == value
			&& vector.Y == value
			&& vector.Z == value;
	}

	public static bool Any(Vector3I vector, int value)
	{
		return vector.X == value
			|| vector.Y == value
			|| vector.Z == value;
	}

	public static Vector3I BitwiseAnd(Vector3I left, Vector3I right)
	{
		return left & right;
	}

	public static Vector3I BitwiseOr(Vector3I left, Vector3I right)
	{
		return left | right;
	}

	public static Vector3I Clamp(Vector3I value, Vector3I min, Vector3I max)
	{
		return new(
			Math.Clamp(value.X, min.X, max.X),
			Math.Clamp(value.Y, min.Y, max.Y),
			Math.Clamp(value.Z, min.Z, max.Z)
		);
	}

	public static Vector3I CopySign(Vector3I value, Vector3I sign)
	{
		return new(
			sign.X >= 0 ? Math.Abs(value.X) : -Math.Abs(value.X),
			sign.Y >= 0 ? Math.Abs(value.Y) : -Math.Abs(value.Y),
			sign.Z >= 0 ? Math.Abs(value.Z) : -Math.Abs(value.Z)
		);
	}

	public static int Count(Vector3I vector, int value)
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
		if (vector.Z == value)
		{
			count++;
		}
		return count;
	}

	public static Vector3I Create(int value)
	{
		return new(value);
	}

	public static Vector3I Create(int x, int y, int z)
	{
		return new(x, y, z);
	}

	public static Vector3I Create(ReadOnlySpan<int> values)
	{
		return new(values);
	}

	public static Vector3I Create(Vector2I vector, int z)
	{
		return new(vector, z);
	}

	public static Vector3I CreateScalar(int x)
	{
		return new(x, 0, 0);
	}

	public static Vector3I Cross(Vector3I left, Vector3I right)
	{
		return new(
			left.Y * right.Z - left.Z * right.Y,
			left.Z * right.X - left.X * right.Z,
			left.X * right.Y - left.Y * right.X
		);
	}

	public static float Distance(Vector3I left, Vector3I right)
	{
		return (float)Math.Sqrt(DistanceSquared(left, right));
	}

	public static long DistanceSquared(Vector3I left, Vector3I right)
	{
		return (left - right).LengthSquared();
	}

	public static Vector3I Divide(Vector3I left, int right)
	{
		return left / right;
	}

	public static Vector3I Divide(Vector3I left, Vector3I right)
	{
		return left / right;
	}

	public static long Dot(Vector3I left, Vector3I right)
	{
		return (long)left.X * right.X
			 + (long)left.Y * right.Y
			 + (long)left.Z * right.Z;
	}

	public static bool EqualsAll(Vector3I left, Vector3I right)
	{
		return left == right;
	}

	public static bool EqualsAny(Vector3I left, Vector3I right)
	{
		return left.X == right.X
			|| left.Y == right.Y
			|| left.Z == right.Z;
	}

	public static bool GreaterThanAll(Vector3I left, Vector3I right)
	{
		return left.X > right.X
			&& left.Y > right.Y
			&& left.Z > right.Z;
	}

	public static bool GreaterThanAny(Vector3I left, Vector3I right)
	{
		return left.X > right.X
			|| left.Y > right.Y
			|| left.Z > right.Z;
	}

	public static bool GreaterThanOrEqualAll(Vector3I left, Vector3I right)
	{
		return left.X >= right.X
			&& left.Y >= right.Y
			&& left.Z >= right.Z;
	}

	public static bool GreaterThanOrEqualAny(Vector3I left, Vector3I right)
	{
		return left.X >= right.X
			|| left.Y >= right.Y
			|| left.Z >= right.Z;
	}

	public static int IndexOf(Vector3I vector, int value)
	{
		if (vector.X == value)
		{
			return 0;
		}
		if (vector.Y == value)
		{
			return 1;
		}
		if (vector.Z == value)
		{
			return 2;
		}
		return -1;
	}

	public static int LastIndexOf(Vector3I vector, int value)
	{
		if (vector.Z == value)
		{
			return 2;
		}
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

	public static bool LessThanAll(Vector3I left, Vector3I right)
	{
		return left.X < right.X
			&& left.Y < right.Y
			&& left.Z < right.Z;
	}

	public static bool LessThanAny(Vector3I left, Vector3I right)
	{
		return left.X < right.X
			|| left.Y < right.Y
			|| left.Z < right.Z;
	}

	public static bool LessThanOrEqualAll(Vector3I left, Vector3I right)
	{
		return left.X <= right.X
			&& left.Y <= right.Y
			&& left.Z <= right.Z;
	}

	public static bool LessThanOrEqualAny(Vector3I left, Vector3I right)
	{
		return left.X <= right.X
			|| left.Y <= right.Y
			|| left.Z <= right.Z;
	}

	public static unsafe Vector3I Load(int* source)
	{
		return *(Vector3I*)source;
	}

	public static Vector3I LoadUnsafe(ref readonly int source, nuint elementOffset)
	{
		return Unsafe.As<int, Vector3I>(ref Unsafe.Add(ref Unsafe.AsRef(in source), (nint)elementOffset));
	}

	public static Vector3I LoadUnsafe(ref readonly int source)
	{
		return Unsafe.As<int, Vector3I>(ref Unsafe.AsRef(in source));
	}

	public static Vector3I Max(Vector3I value1, Vector3I value2)
	{
		return new(
			Math.Max(value1.X, value2.X),
			Math.Max(value1.Y, value2.Y),
			Math.Max(value1.Z, value2.Z)
		);
	}

	public static Vector3I MaxMagnitude(Vector3I value1, Vector3I value2)
	{
		return new(
			Math.Abs(value1.X) > Math.Abs(value2.X) ? value1.X : value2.X,
			Math.Abs(value1.Y) > Math.Abs(value2.Y) ? value1.Y : value2.Y,
			Math.Abs(value1.Z) > Math.Abs(value2.Z) ? value1.Z : value2.Z
		);
	}

	public static Vector3I Min(Vector3I value1, Vector3I value2)
	{
		return new(
			Math.Min(value1.X, value2.X),
			Math.Min(value1.Y, value2.Y),
			Math.Min(value1.Z, value2.Z)
		);
	}

	public static Vector3I MinMagnitude(Vector3I value1, Vector3I value2)
	{
		return new(
			Math.Abs(value1.X) < Math.Abs(value2.X) ? value1.X : value2.X,
			Math.Abs(value1.Y) < Math.Abs(value2.Y) ? value1.Y : value2.Y,
			Math.Abs(value1.Z) < Math.Abs(value2.Z) ? value1.Z : value2.Z
		);
	}

	public static Vector3I Multiply(int left, Vector3I right)
	{
		return left * right;
	}

	public static Vector3I Multiply(Vector3I left, int right)
	{
		return left * right;
	}

	public static Vector3I Multiply(Vector3I left, Vector3I right)
	{
		return left * right;
	}

	public static Vector3I Negate(Vector3I value)
	{
		return -value;
	}

	public static bool None(Vector3I vector, int value)
	{
		return vector.X != value
			&& vector.Y != value
			&& vector.Z != value;
	}

	public static Vector3I OnesComplement(Vector3I value)
	{
		return ~value;
	}

	public static Vector3I Shuffle(Vector3I vector, byte xIndex, byte yIndex, byte zIndex)
	{
		return new(
			vector[xIndex],
			vector[yIndex],
			vector[zIndex]
		);
	}

	public static Vector3I Subtract(Vector3I left, Vector3I right)
	{
		return left - right;
	}

	public static long Sum(Vector3I value)
	{
		return (long)value.X + value.Y + value.Z;
	}

	public static Vector3I Xor(Vector3I left, Vector3I right)
	{
		return left ^ right;
	}

	public static Vector3I DivideAndRoundUp(Vector3I value, Vector3I divisor)
	{
		return new(
			(value.X + divisor.X - 1) / divisor.X,
			(value.Y + divisor.Y - 1) / divisor.Y,
			(value.Z + divisor.Z - 1) / divisor.Z
		);
	}

	public static Vector3I Sign(Vector3I value)
	{
		return new(
			Math.Sign(value.X),
			Math.Sign(value.Y),
			Math.Sign(value.Z)
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
		Unsafe.As<int, Vector3I>(ref array[0]) = this;
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

		Unsafe.As<int, Vector3I>(ref array[index]) = this;
	}

	public readonly void CopyTo(Span<int> destination)
	{
		if (destination.Length < ELEMENT_COUNT)
		{
			throw new ArgumentException($"{nameof(destination)} must contain at least {ELEMENT_COUNT} elements, but it only contains {destination.Length}", nameof(destination));
		}
		Unsafe.As<int, Vector3I>(ref MemoryMarshal.GetReference(destination)) = this;
	}

	public readonly bool TryCopyTo(Span<int> destination)
	{
		if (destination.Length < ELEMENT_COUNT)
		{
			return false;
		}

		Unsafe.As<int, Vector3I>(ref MemoryMarshal.GetReference(destination)) = this;
		return true;
	}

	public override readonly bool Equals([NotNullWhen(true)] object? obj)
	{
		return obj is Vector3I vector && Equals(vector);
	}

	public readonly bool Equals(Vector3I other)
	{
		return this == other;
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(X, Y, Z);
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
		handler.AppendLiteral(separator);
		handler.AppendLiteral(" ");
		handler.AppendFormatted(Z, format);
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
		if (!Utility.TryAppend(separator, destination, ref written))
		{
			return false;
		}
		if (!Utility.TryAppend(" ", destination, ref written))
		{
			return false;
		}
		if (!Utility.TryAppend(Z, destination, ref written, format, provider))
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
		if (X <= Y)
		{
			return X <= Z ? 0 : 2;
		}
		else
		{
			return Y <= Z ? 1 : 2;
		}
	}

	public readonly int MaxAxisIndex()
	{
		if (X >= Y)
		{
			return X >= Z ? 0 : 2;
		}
		else
		{
			return Y >= Z ? 1 : 2;
		}
	}

	public readonly void Deconstruct(out int x, out int y, out int z)
	{
		x = X;
		y = Y;
		z = Z;
	}

	#endregion

	#region Operators

	public static Vector3I operator +(Vector3I value)
	{
		return value;
	}

	public static Vector3I operator -(Vector3I value)
	{
		return new(
			-value.X,
			-value.Y,
			-value.Z
		);
	}

	public static Vector3I operator +(Vector3I left, Vector3I right)
	{
		return new(
			left.X + right.X,
			left.Y + right.Y,
			left.Z + right.Z
		);
	}

	public static Vector3I operator -(Vector3I left, Vector3I right)
	{
		return new(
			left.X - right.X,
			left.Y - right.Y,
			left.Z - right.Z
		);
	}

	public static Vector3I operator *(int left, Vector3I right)
	{
		return new(
			left * right.X,
			left * right.Y,
			left * right.Z
		);
	}

	public static Vector3I operator *(Vector3I left, int right)
	{
		return new(
			left.X * right,
			left.Y * right,
			left.Z * right
		);
	}

	public static Vector3I operator *(Vector3I left, Vector3I right)
	{
		return new(
			left.X * right.X,
			left.Y * right.Y,
			left.Z * right.Z
		);
	}

	public static Vector3I operator /(Vector3I left, int right)
	{
		return new(
			left.X / right,
			left.Y / right,
			left.Z / right
		);
	}

	public static Vector3I operator /(Vector3I left, Vector3I right)
	{
		return new(
			left.X / right.X,
			left.Y / right.Y,
			left.Z / right.Z
		);
	}

	public static Vector3I operator ~(Vector3I value)
	{
		return new(
			~value.X,
			~value.Y,
			~value.Z
		);
	}

	public static Vector3I operator &(Vector3I left, Vector3I right)
	{
		return new(
			left.X & right.X,
			left.Y & right.Y,
			left.Z & right.Z
		);
	}

	public static Vector3I operator |(Vector3I left, Vector3I right)
	{
		return new(
			left.X | right.X,
			left.Y | right.Y,
			left.Z | right.Z
		);
	}

	public static Vector3I operator ^(Vector3I left, Vector3I right)
	{
		return new(
			left.X ^ right.X,
			left.Y ^ right.Y,
			left.Z ^ right.Z
		);
	}

	public static Vector3I operator <<(Vector3I value, int shiftAmount)
	{
		return new(
			value.X << shiftAmount,
			value.Y << shiftAmount,
			value.Z << shiftAmount
		);
	}

	public static Vector3I operator >>(Vector3I value, int shiftAmount)
	{
		return new(
			value.X >> shiftAmount,
			value.Y >> shiftAmount,
			value.Z >> shiftAmount
		);
	}

	public static Vector3I operator >>>(Vector3I value, int shiftAmount)
	{
		return new(
			value.X >>> shiftAmount,
			value.Y >>> shiftAmount,
			value.Z >>> shiftAmount
		);
	}

	public static bool operator ==(Vector3I left, Vector3I right)
	{
		return left.X == right.X
			&& left.Y == right.Y
			&& left.Z == right.Z;
	}

	public static bool operator !=(Vector3I left, Vector3I right)
	{
		return left.X != right.X
			|| left.Y != right.Y
			|| left.Z != right.Z;
	}

	public static implicit operator Vector3(Vector3I value)
	{
		return new(value.X, value.Y, value.Z);
	}

	public static explicit operator Vector3I(Vector3 value)
	{
		return new((int)value.X, (int)value.Y, (int)value.Z);
	}

	#endregion
}
