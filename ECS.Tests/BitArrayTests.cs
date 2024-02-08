namespace ECS.Tests;

public sealed class BitArrayTests
{
	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(100)]
	public void ShouldCreateWithAllProperValues(int size)
	{
		BitArray falseBitArray = new(size);
		for (int i = 0; i < size; i++)
			falseBitArray[i].Should().BeFalse();
		
		BitArray trueBitArray = new(size, true);
		for (int i = 0; i < size; i++)
			trueBitArray[i].Should().BeTrue();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(100)]
	public void ShouldSetValue(int index)
	{
		BitArray bitArray = new(index + 50);
		bitArray[index] = true;
		bitArray[index].Should().BeTrue();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(7)]
	[InlineData(8)]
	[InlineData(100)]
	public void ShouldSetOnlyValue(int index)
	{
		BitArray bitArray = new(index + 50);
		bitArray[index] = true;
		for (int i = 0; i < bitArray.Size; i++)
		{
			if (i != index)
				bitArray[i].Should().BeFalse();
		}
	}

	[Fact]
	public void ShouldBeEqual()
	{
		BitArray bitArray1 = new(100);
		bitArray1[30] = true;
		bitArray1[43] = true;
		BitArray bitArray2 = new(100);
		bitArray2[30] = true;
		bitArray2[43] = true;
		bitArray1.Should().Be(bitArray2);
		bitArray1.GetHashCode().Should().Be(bitArray2.GetHashCode());
	}

	[Fact]
	public void ShouldNotBeEqual()
	{
		BitArray bitArray1 = new(100);
		bitArray1[30] = true;
		bitArray1[43] = true;
		BitArray bitArray2 = new(100);
		bitArray2[30] = true;
		//bitArray2[43] = true;
		bitArray1.Should().NotBe(bitArray2);
		bitArray1.GetHashCode().Should().NotBe(bitArray2.GetHashCode());
	}
}