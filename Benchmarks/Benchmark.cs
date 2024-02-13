using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public abstract class Benchmark
{
	[Params(10, 10_000, 1_000_000)]
	public int EntitiesCount { get; set; }
}