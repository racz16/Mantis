using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Mantis.Core.Numerics;

[StructLayout(LayoutKind.Sequential)]
public struct Vector4I : IEquatable<Vector4I>, IFormattable, ISpanFormattable
{
	#region Static properties

	public static Vector4I Zero => default;
	public static Vector4I One => new(1);
	public static Vector4I UnitX => new(1, 0, 0, 0);
	public static Vector4I UnitY => new(0, 1, 0, 0);
	public static Vector4I UnitZ => new(0, 0, 1, 0);
	public static Vector4I UnitW => new(0, 0, 0, 1);

	#endregion

	#region Fields

	public int X;
	public int Y;
	public int Z;
	public int W;

	private const int ELEMENT_COUNT = 4;

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
				3 => W,
				_ => throw new ArgumentOutOfRangeException(nameof(index), $"You can only index a Vector4I with 0, 1, 2, or 3, not with {index}"),
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
				case 3:
					W = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(index), $"You can only index a Vector4I with 0, 1, 2, or 3, not with {index}");
			}
		}
	}

	#endregion

	#region Constructors

	public Vector4I(int value)
	{
		X = value;
		Y = value;
		Z = value;
		W = value;
	}

	public Vector4I(int x, int y, int z, int w)
	{
		X = x;
		Y = y;
		Z = z;
		W = w;
	}

	public Vector4I(ReadOnlySpan<int> values)
	{
		if (values.Length < ELEMENT_COUNT)
		{
			throw new ArgumentException($"{nameof(values)} must contain at least {ELEMENT_COUNT} elements, but it only contains {values.Length}", nameof(values));
		}
		this = Unsafe.ReadUnaligned<Vector4I>(ref Unsafe.As<int, byte>(ref MemoryMarshal.GetReference(values)));
	}

	public Vector4I(Vector2I value, int z, int w)
	{
		X = value.X;
		Y = value.Y;
		Z = z;
		W = w;
	}

	public Vector4I(Vector3I value, int w)
	{
		X = value.X;
		Y = value.Y;
		Z = value.Z;
		W = w;
	}

	#endregion

	#region Static methods

	public static Vector4I Abs(Vector4I value)
	{
		return new(
			Math.Abs(value.X),
			Math.Abs(value.Y),
			Math.Abs(value.Z),
			Math.Abs(value.W)
		);
	}

	public static Vector4I Add(Vector4I left, Vector4I right)
	{
		return left + right;
	}

	public static bool All(Vector4I vector, int value)
	{
		return vector.X == value
			&& vector.Y == value
			&& vector.Z == value
			&& vector.W == value;
	}

	public static bool Any(Vector4I vector, int value)
	{
		return vector.X == value
			|| vector.Y == value
			|| vector.Z == value
			|| vector.W == value;
	}

	public static Vector4I BitwiseAnd(Vector4I left, Vector4I right)
	{
		return left & right;
	}

	public static Vector4I BitwiseOr(Vector4I left, Vector4I right)
	{
		return left | right;
	}

	public static Vector4I Clamp(Vector4I value, Vector4I min, Vector4I max)
	{
		return new(
			Math.Clamp(value.X, min.X, max.X),
			Math.Clamp(value.Y, min.Y, max.Y),
			Math.Clamp(value.Z, min.Z, max.Z),
			Math.Clamp(value.W, min.W, max.W)
		);
	}

	public static Vector4I CopySign(Vector4I value, Vector4I sign)
	{
		return new(
			sign.X >= 0 ? Math.Abs(value.X) : -Math.Abs(value.X),
			sign.Y >= 0 ? Math.Abs(value.Y) : -Math.Abs(value.Y),
			sign.Z >= 0 ? Math.Abs(value.Z) : -Math.Abs(value.Z),
			sign.W >= 0 ? Math.Abs(value.W) : -Math.Abs(value.W)
		);
	}

	public static int Count(Vector4I vector, int value)
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
		if (vector.W == value)
		{
			count++;
		}
		return count;
	}

	public static Vector4I Create(int value)
	{
		return new(value);
	}

	public static Vector4I Create(int x, int y, int z, int w)
	{
		return new(x, y, z, w);
	}

	public static Vector4I Create(ReadOnlySpan<int> values)
	{
		return new(values);
	}

	public static Vector4I Create(Vector2I vector, int z, int w)
	{
		return new(vector, z, w);
	}

	public static Vector4I Create(Vector3I vector, int w)
	{
		return new(vector, w);
	}

	public static Vector4I CreateScalar(int x)
	{
		return new(x, 0, 0, 0);
	}

	public static Vector4I Cross(Vector4I left, Vector4I right)
	{
		return new(
			left.Y * right.Z - left.Z * right.Y,
			left.Z * right.X - left.X * right.Z,
			left.X * right.Y - left.Y * right.X,
			left.W * right.W
		);
	}

	public static float Distance(Vector4I left, Vector4I right)
	{
		return (float)Math.Sqrt(DistanceSquared(left, right));
	}

	public static long DistanceSquared(Vector4I left, Vector4I right)
	{
		return (left - right).LengthSquared();
	}

	public static Vector4I Divide(Vector4I left, int right)
	{
		return left / right;
	}

	public static Vector4I Divide(Vector4I left, Vector4I right)
	{
		return left / right;
	}

	public static long Dot(Vector4I left, Vector4I right)
	{
		return (long)left.X * right.X
			 + (long)left.Y * right.Y
			 + (long)left.Z * right.Z
			 + (long)left.W * right.W;
	}

	public static bool EqualsAll(Vector4I left, Vector4I right)
	{
		return left == right;
	}

	public static bool EqualsAny(Vector4I left, Vector4I right)
	{
		return left.X == right.X
			|| left.Y == right.Y
			|| left.Z == right.Z
			|| left.W == right.W;
	}

	public static bool GreaterThanAll(Vector4I left, Vector4I right)
	{
		return left.X > right.X
			&& left.Y > right.Y
			&& left.Z > right.Z
			&& left.W > right.W;
	}

	public static bool GreaterThanAny(Vector4I left, Vector4I right)
	{
		return left.X > right.X
			|| left.Y > right.Y
			|| left.Z > right.Z
			|| left.W > right.W;
	}

	public static bool GreaterThanOrEqualAll(Vector4I left, Vector4I right)
	{
		return left.X >= right.X
			&& left.Y >= right.Y
			&& left.Z >= right.Z
			&& left.W >= right.W;
	}

	public static bool GreaterThanOrEqualAny(Vector4I left, Vector4I right)
	{
		return left.X >= right.X
			|| left.Y >= right.Y
			|| left.Z >= right.Z
			|| left.W >= right.W;
	}

	public static int IndexOf(Vector4I vector, int value)
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
		if (vector.W == value)
		{
			return 3;
		}
		return -1;
	}

	public static int LastIndexOf(Vector4I vector, int value)
	{
		if (vector.W == value)
		{
			return 3;
		}
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

	public static bool LessThanAll(Vector4I left, Vector4I right)
	{
		return left.X < right.X
			&& left.Y < right.Y
			&& left.Z < right.Z
			&& left.W < right.W;
	}

	public static bool LessThanAny(Vector4I left, Vector4I right)
	{
		return left.X < right.X
			|| left.Y < right.Y
			|| left.Z < right.Z
			|| left.W < right.W;
	}

	public static bool LessThanOrEqualAll(Vector4I left, Vector4I right)
	{
		return left.X <= right.X
			&& left.Y <= right.Y
			&& left.Z <= right.Z
			&& left.W <= right.W;
	}

	public static bool LessThanOrEqualAny(Vector4I left, Vector4I right)
	{
		return left.X <= right.X
			|| left.Y <= right.Y
			|| left.Z <= right.Z
			|| left.W <= right.W;
	}

	public static unsafe Vector4I Load(int* source)
	{
		return *(Vector4I*)source;
	}

	public static Vector4I LoadUnsafe(ref readonly int source, nuint elementOffset)
	{
		return Unsafe.As<int, Vector4I>(ref Unsafe.Add(ref Unsafe.AsRef(in source), (nint)elementOffset));
	}

	public static Vector4I LoadUnsafe(ref readonly int source)
	{
		return Unsafe.As<int, Vector4I>(ref Unsafe.AsRef(in source));
	}

	public static Vector4I Max(Vector4I value1, Vector4I value2)
	{
		return new(
			Math.Max(value1.X, value2.X),
			Math.Max(value1.Y, value2.Y),
			Math.Max(value1.Z, value2.Z),
			Math.Max(value1.W, value2.W)
		);
	}

	public static Vector4I MaxMagnitude(Vector4I value1, Vector4I value2)
	{
		return new(
			Math.Abs(value1.X) > Math.Abs(value2.X) ? value1.X : value2.X,
			Math.Abs(value1.Y) > Math.Abs(value2.Y) ? value1.Y : value2.Y,
			Math.Abs(value1.Z) > Math.Abs(value2.Z) ? value1.Z : value2.Z,
			Math.Abs(value1.W) > Math.Abs(value2.W) ? value1.W : value2.W
		);
	}

	public static Vector4I Min(Vector4I value1, Vector4I value2)
	{
		return new(
			Math.Min(value1.X, value2.X),
			Math.Min(value1.Y, value2.Y),
			Math.Min(value1.Z, value2.Z),
			Math.Min(value1.W, value2.W)
		);
	}

	public static Vector4I MinMagnitude(Vector4I value1, Vector4I value2)
	{
		return new(
			Math.Abs(value1.X) < Math.Abs(value2.X) ? value1.X : value2.X,
			Math.Abs(value1.Y) < Math.Abs(value2.Y) ? value1.Y : value2.Y,
			Math.Abs(value1.Z) < Math.Abs(value2.Z) ? value1.Z : value2.Z,
			Math.Abs(value1.W) < Math.Abs(value2.W) ? value1.W : value2.W
		);
	}

	public static Vector4I Multiply(int left, Vector4I right)
	{
		return left * right;
	}

	public static Vector4I Multiply(Vector4I left, int right)
	{
		return left * right;
	}

	public static Vector4I Multiply(Vector4I left, Vector4I right)
	{
		return left * right;
	}

	public static Vector4I Negate(Vector4I value)
	{
		return -value;
	}

	public static bool None(Vector4I vector, int value)
	{
		return vector.X != value
			&& vector.Y != value
			&& vector.Z != value
			&& vector.W != value;
	}

	public static Vector4I OnesComplement(Vector4I value)
	{
		return ~value;
	}

	public static Vector4I Shuffle(Vector4I vector, byte xIndex, byte yIndex, byte zIndex, byte wIndex)
	{
		return new(
			vector[xIndex],
			vector[yIndex],
			vector[zIndex],
			vector[wIndex]
		);
	}

	public static Vector4I Subtract(Vector4I left, Vector4I right)
	{
		return left - right;
	}

	public static long Sum(Vector4I value)
	{
		return (long)value.X + value.Y + value.Z + value.W;
	}

	public static Vector4I Xor(Vector4I left, Vector4I right)
	{
		return left ^ right;
	}

	public static Vector4I DivideAndRoundUp(Vector4I value, Vector4I divisor)
	{
		return new(
			(value.X + divisor.X - 1) / divisor.X,
			(value.Y + divisor.Y - 1) / divisor.Y,
			(value.Z + divisor.Z - 1) / divisor.Z,
			(value.W + divisor.W - 1) / divisor.W
		);
	}

	public static Vector4I Sign(Vector4I value)
	{
		return new(
			Math.Sign(value.X),
			Math.Sign(value.Y),
			Math.Sign(value.Z),
			Math.Sign(value.W)
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
		Unsafe.As<int, Vector4I>(ref array[0]) = this;
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

		Unsafe.As<int, Vector4I>(ref array[index]) = this;
	}

	public readonly void CopyTo(Span<int> destination)
	{
		if (destination.Length < ELEMENT_COUNT)
		{
			throw new ArgumentException($"{nameof(destination)} must contain at least {ELEMENT_COUNT} elements, but it only contains {destination.Length}", nameof(destination));
		}
		Unsafe.As<int, Vector4I>(ref MemoryMarshal.GetReference(destination)) = this;
	}

	public readonly bool TryCopyTo(Span<int> destination)
	{
		if (destination.Length < ELEMENT_COUNT)
		{
			return false;
		}

		Unsafe.As<int, Vector4I>(ref MemoryMarshal.GetReference(destination)) = this;
		return true;
	}

	public override readonly bool Equals([NotNullWhen(true)] object? obj)
	{
		return obj is Vector4I vector && Equals(vector);
	}

	public readonly bool Equals(Vector4I other)
	{
		return this == other;
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(X, Y, Z, W);
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
		handler.AppendLiteral(separator);
		handler.AppendLiteral(" ");
		handler.AppendFormatted(W, format);
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
		if (!Utility.TryAppend(separator, destination, ref written))
		{
			return false;
		}
		if (!Utility.TryAppend(" ", destination, ref written))
		{
			return false;
		}
		if (!Utility.TryAppend(W, destination, ref written, format, provider))
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
			if (X <= Z)
			{
				return X <= W ? 0 : 3;
			}
			else
			{
				return Z <= W ? 2 : 3;
			}
		}
		else
		{
			if (Y <= Z)
			{
				return Y <= W ? 1 : 3;
			}
			else
			{
				return Z <= W ? 2 : 3;
			}
		}
	}

	public readonly int MaxAxisIndex()
	{
		if (X >= Y)
		{
			if (X >= Z)
			{
				return X >= W ? 0 : 3;
			}
			else
			{
				return Z >= W ? 2 : 3;
			}
		}
		else
		{
			if (Y >= Z)
			{
				return Y >= W ? 1 : 3;
			}
			else
			{
				return Z >= W ? 2 : 3;
			}
		}
	}

	public readonly void Deconstruct(out int x, out int y, out int z, out int w)
	{
		x = X;
		y = Y;
		z = Z;
		w = W;
	}

	#endregion

	#region Operators

	public static Vector4I operator +(Vector4I value)
	{
		return value;
	}

	public static Vector4I operator -(Vector4I value)
	{
		return new(
			-value.X,
			-value.Y,
			-value.Z,
			-value.W
		);
	}

	public static Vector4I operator +(Vector4I left, Vector4I right)
	{
		return new(
			left.X + right.X,
			left.Y + right.Y,
			left.Z + right.Z,
			left.W + right.W
		);
	}

	public static Vector4I operator -(Vector4I left, Vector4I right)
	{
		return new(
			left.X - right.X,
			left.Y - right.Y,
			left.Z - right.Z,
			left.W - right.W
		);
	}

	public static Vector4I operator *(int left, Vector4I right)
	{
		return new(
			left * right.X,
			left * right.Y,
			left * right.Z,
			left * right.W
		);
	}

	public static Vector4I operator *(Vector4I left, int right)
	{
		return new(
			left.X * right,
			left.Y * right,
			left.Z * right,
			left.W * right
		);
	}

	public static Vector4I operator *(Vector4I left, Vector4I right)
	{
		return new(
			left.X * right.X,
			left.Y * right.Y,
			left.Z * right.Z,
			left.W * right.W
		);
	}

	public static Vector4I operator /(Vector4I left, int right)
	{
		return new(
			left.X / right,
			left.Y / right,
			left.Z / right,
			left.W / right
		);
	}

	public static Vector4I operator /(Vector4I left, Vector4I right)
	{
		return new(
			left.X / right.X,
			left.Y / right.Y,
			left.Z / right.Z,
			left.W / right.W
		);
	}

	public static Vector4I operator ~(Vector4I value)
	{
		return new(
			~value.X,
			~value.Y,
			~value.Z,
			~value.W
		);
	}

	public static Vector4I operator &(Vector4I left, Vector4I right)
	{
		return new(
			left.X & right.X,
			left.Y & right.Y,
			left.Z & right.Z,
			left.W & right.W
		);
	}

	public static Vector4I operator |(Vector4I left, Vector4I right)
	{
		return new(
			left.X | right.X,
			left.Y | right.Y,
			left.Z | right.Z,
			left.W | right.W
		);
	}

	public static Vector4I operator ^(Vector4I left, Vector4I right)
	{
		return new(
			left.X ^ right.X,
			left.Y ^ right.Y,
			left.Z ^ right.Z,
			left.W ^ right.W
		);
	}

	public static Vector4I operator <<(Vector4I value, int shiftAmount)
	{
		return new(
			value.X << shiftAmount,
			value.Y << shiftAmount,
			value.Z << shiftAmount,
			value.W << shiftAmount
		);
	}

	public static Vector4I operator >>(Vector4I value, int shiftAmount)
	{
		return new(
			value.X >> shiftAmount,
			value.Y >> shiftAmount,
			value.Z >> shiftAmount,
			value.W >> shiftAmount
		);
	}

	public static Vector4I operator >>>(Vector4I value, int shiftAmount)
	{
		return new(
			value.X >>> shiftAmount,
			value.Y >>> shiftAmount,
			value.Z >>> shiftAmount,
			value.W >>> shiftAmount
		);
	}

	public static bool operator ==(Vector4I left, Vector4I right)
	{
		return left.X == right.X
			&& left.Y == right.Y
			&& left.Z == right.Z
			&& left.W == right.W;
	}

	public static bool operator !=(Vector4I left, Vector4I right)
	{
		return left.X != right.X
			|| left.Y != right.Y
			|| left.Z != right.Z
			|| left.W != right.W;
	}

	public static implicit operator Vector4(Vector4I value)
	{
		return new(value.X, value.Y, value.Z, value.W);
	}

	public static explicit operator Vector4I(Vector4 value)
	{
		return new((int)value.X, (int)value.Y, (int)value.Z, (int)value.W);
	}

	#endregion
}
