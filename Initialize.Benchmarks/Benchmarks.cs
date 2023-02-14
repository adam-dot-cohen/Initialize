using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Microsoft.IO;

namespace Initialize.Benchmarks;
[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1, runtimeMoniker: RuntimeMoniker.Net70)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ChampionChallengerBenchmarks
{
    private static readonly RecyclableMemoryStreamManager manager = new();
    private readonly byte[] chunk = new byte[128];

    [Params(100, 10_000, 100_000)]
    public int TotalCount;

    [Benchmark(Description = "ResizableSpanWriter")]
    public void Benchmark()
    {
    }
}
