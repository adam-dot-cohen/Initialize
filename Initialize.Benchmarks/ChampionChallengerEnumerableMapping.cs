using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;

namespace Initialize.Benchmarks;

[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1, iterationCount:10,
    runtimeMoniker: RuntimeMoniker.Net70)]
[MemoryDiagnoser]
[HideColumns(Column.StdDev, Column.Median, Column.Error, Column.RatioSD)]
public class ChampionChallengerEnumerableMapping
{
    private AutoMapper.Mapper _autoMapper;
    private List<Test> _testObjects;
    private IEnumerable<Test> _testEnumerable;

    public ChampionChallengerEnumerableMapping()
    {
        //Initialize mapper
        var config = new MapperConfiguration(cfg =>
            cfg.CreateMap<Test, Test2>());
        _autoMapper = new AutoMapper.Mapper(config);

        //Initialize mapper
        Mapper<Test, Test2>.Map(testObj);

        _testObjects = Enumerable.Range(0, 10000).Select(r => new Test() { PropString = r.ToString(), Prop = r }).ToList();
    }
    [Params(100_000, 1_000_000)]
    public int Iterations { get; set; }

    [IterationSetup]
    public void Setup()
    {
        _testEnumerable = Enumerable.Range(0, Iterations)
            .Select(r => new Test() { PropString = r.ToString(), Prop = r });

        _testObjects = _testEnumerable.ToList();
    }

    private Test testObj = new Test()
    {
        Prop = 1,
        PropString = "a",
    };
    [Benchmark(Description = "Initialize_List", Baseline = true)]
    public void Mapper()
    {
        var test2 = new Test2();

        var result = Mapper<Test, Test2>.Map(_testObjects);

        Debug.Assert(result.Count() == _testObjects.Count);
    }

    [Benchmark(Description = "AutoMapper_List")]
    public void AutoMapper()
    {
        var test2 = new Test2();

        var result = _autoMapper.Map<List<Test>>(_testObjects);

        Debug.Assert(result.Count() == _testObjects.Count);
    }
    [Benchmark(Description = "Initialize_IEnumerable")]
    public void MapperEnumerable()
    {
        var test2 = new Test2();

        var result = Mapper<Test, Test2>.Map(_testEnumerable);

        Debug.Assert(result.Count() == _testObjects.Count);
    }

    [Benchmark(Description = "AutoMapper_IEnumerable")]
    public void AutoMapperEnumerable()
    {
        var test2 = new Test2();

        var result = _autoMapper.Map<List<Test>>(_testEnumerable);

        Debug.Assert(result.Count() == _testObjects.Count);
    }
}