using BenchmarkDotNet.Attributes;

namespace Benchmarks;

public partial class EntitiesIteration
{
	[GlobalSetup(Target = nameof(NoECS))]
	public void SetupNoECS()
	{
		_noECSCharacters = new Character[EntitiesCount];
		for (var i = 0; i < _noECSCharacters.Length; i++)
			_noECSCharacters[i] = new Character();
	}

	[Benchmark(Baseline = true)]
	public void NoECS()
	{
		foreach (var character in _noECSCharacters)
			character.Update();
	}
	
	private Character[] _noECSCharacters;
}