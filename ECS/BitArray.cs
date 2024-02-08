using CommunityToolkit.Diagnostics;

namespace ECS;

/// <summary>
/// An alternative to <see cref="System.Collections.BitArray"/>
/// </summary>
internal readonly struct BitArray
{
	private static int GetByteIndex(int bit) => bit / 8;
	private static int GetBitIndex(int bit) => bit % 8;

	public readonly int Size;
	
	public BitArray(int size, bool defaultValue = false)
	{
		Size = size;
		_data = new byte[size.DivideAndRoundUp(8)];
		SetAll(defaultValue);
	}

	public void SetAll(bool value)
	{
		var byteValue = (byte)(value ? 0b11111111 : 0b00000000);
		Array.Fill(_data, byteValue);
	}
	
	public bool this[int index]
	{
		get
		{
			Guard.IsLessThan(index, Size);
			int byteIndex = GetByteIndex(index);
			int bitIndex = GetBitIndex(index);
			byte byteValue = _data[byteIndex];
			var bitMask = 1 << bitIndex;
			return (byteValue & bitMask) > 0;
		}
		set
		{
			Guard.IsLessThan(index, Size);
			int byteIndex = GetByteIndex(index);
			int bitIndex = GetBitIndex(index);
			byte byteValue = _data[byteIndex];
			var bitMask = 1 << bitIndex;
			_data[byteIndex] = value ? (byte)(byteValue | bitMask) : (byte)(byteValue & ~bitMask);
		}
	}

	public override int GetHashCode()
	{
		HashCode hashCode = new();
		hashCode.AddBytes(_data.AsSpan());
		return hashCode.ToHashCode();
	}

	public override bool Equals(object? obj)
	{
		return obj is BitArray other && Equals(other);
	}

	public bool Equals(BitArray other)
	{
		return Size == other.Size && _data.SequenceEqual(other._data);
	}

	private readonly byte[] _data;
}