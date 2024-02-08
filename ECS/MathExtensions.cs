namespace ECS;

internal static class MathExtensions
{
	public static int DivideAndRoundUp(this int numerator, int denominator)
	{
		return (numerator + denominator - 1) / denominator;
	}
}