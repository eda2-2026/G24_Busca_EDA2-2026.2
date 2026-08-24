using BenchmarkDotNet.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace PrefixContainer.Benchmarks;

[MemoryDiagnoser]
public class PrefixContainerBenchmark
{
    private PrefixContainer _container = null!;
    private string _prefix = null!;

    [Params(100, 1_000, 10_000)]
    public int N { get; set; }

    [Params("missing", "single", "many")]
    public string Scenario { get; set; } = null!;

    [GlobalSetup]
    public void Setup()
    {
        var keys = new List<string>(N);

        for (var i = 0; i < N; i++)
        {
            keys.Add($"item{i:D6}.value");
        }

        switch (Scenario)
        {
            case "missing":
                _prefix = "target";
                break;

            case "single":
                keys[N - 1] = "target.value";
                _prefix = "target";
                break;

            case "many":
                var matches = Math.Max(1, N / 100);
                for (var i = 0; i < matches; i++)
                {
                    keys[N - 1 - i] = $"target[{i}].value";
                }
                _prefix = "target";
                break;

            default:
                throw new InvalidOperationException($"Unknown scenario: {Scenario}");
        }

        _container = new PrefixContainer(keys);
    }

    [Benchmark]
    public IDictionary<string, string> GetKeysFromPrefix() => _container.GetKeysFromPrefix(_prefix);
}
