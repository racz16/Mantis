using System.Globalization;

namespace Mantis.Core.Logging;

[InterpolatedStringHandler]
public ref struct LogInterpolatedStringHandler<TLevel> where TLevel : ILogLevelProvider
{
	private const int FORMATTED_LENGTH_ESTIMATE = 16;

	public char[]? Message { get; private set; }
	public int MessageLength { get; private set; }

	private Span<char> characters;

	public LogInterpolatedStringHandler(int literalLength, int formattedCount, LogCategory category, out bool shouldAppend)
	{
		shouldAppend = Log.IsEnabled(TLevel.Level, category);
		if (shouldAppend)
		{
			Message = ArrayPool<char>.Shared.Rent(literalLength + formattedCount * FORMATTED_LENGTH_ESTIMATE);
			characters = Message;
		}
		else
		{
			characters = [];
		}
	}

	public void AppendLiteral(string v)
	{
		AppendFormatted(v.AsSpan());
	}

	public void AppendFormatted(string? v)
	{
		if (v != null)
		{
			AppendLiteral(v);
		}
	}

	public void AppendFormatted(ReadOnlySpan<char> v)
	{
		if (characters.Length - MessageLength < v.Length)
		{
			Grow(Math.Max(characters.Length * 2, MessageLength + v.Length));
		}
		v.CopyTo(characters[MessageLength..]);
		MessageLength += v.Length;
	}

	public void AppendFormatted<T>(T v, string? format = null) where T : ISpanFormattable
	{
		Debug.Assert(characters.Length > 0);
		for (int i = 1; i <= 10; i++)
		{
			if (v.TryFormat(characters[MessageLength..], out int charsWritten, format, CultureInfo.InvariantCulture))
			{
				MessageLength += charsWritten;
				return;
			}
			Grow(characters.Length * 2);
		}
		Debug.Fail(null);
	}

	public void AppendFormatted(object? v)
	{
		if (v != null)
		{
			AppendFormatted(v.ToString());
		}
	}

	private void Grow(int size)
	{
		Debug.Assert(size > 0);
		Debug.Assert(Message != null);
		char[] newArray = ArrayPool<char>.Shared.Rent(size);
		Message.AsSpan(0, MessageLength).CopyTo(newArray);
		ArrayPool<char>.Shared.Return(Message);
		Message = newArray;
		characters = Message;
	}
}
