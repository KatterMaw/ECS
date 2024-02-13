using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Diagnostics.dotTrace;

namespace Benchmarks;

[MemoryDiagnoser]
[DotTraceDiagnoser]
[HardwareCounters(HardwareCounter.CacheMisses)]
public partial class EntitiesIteration : Benchmark
{
	
}