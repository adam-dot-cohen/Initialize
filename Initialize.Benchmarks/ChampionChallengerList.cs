using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using System.Collections.Generic;
using System.Linq;
using MapperA = AutoMapper.Mapper;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace Initialize.Benchmarks;

[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1, iterationCount:10,
    runtimeMoniker: RuntimeMoniker.Net70)]
[MemoryDiagnoser]
[HideColumns(Column.StdDev, Column.Median, Column.Error, Column.RatioSD, Column.Gen0, Column.Gen1, Column.Gen2)]
public class ChampionChallengerList
{
    private MapperA _autoMapper;
    private List<Test> _testObjects;
    private IEnumerable<Test> _testEnumerable;

    public ChampionChallengerList()
    {
        //Initialize mapper
        var config = new MapperConfiguration(cfg =>
            cfg.CreateMap<Test, Test2>());
        _autoMapper = new MapperA(config);

        //Initialize mapper
        Mapper<Test, Test2>.Map(testObj);

        _testObjects = Enumerable.Range(0, 10000).Select(r => new Test() { PropString = r.ToString(), Prop = r }).ToList();
    }
    [Params(10000)]//100_000, 1_000_000)]
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

    //[Benchmark(Description = "Mapper", Baseline = true)]
    //public void Mapper()
    //{
    //    var test2 = new Test2();

    //    var result = Mapper<Test, Test2>.Map(_testObjects);

    //    Debug.Assert(result.Count() == _testObjects.Count());
    //}

    //[Benchmark(Description = "AutoMapper")]
    //public void AutoMapper()
    //{
    //    var test2 = new Test2();

    //    var result = _autoMapper.Map<List<Test>>(_testObjects);

    //    Debug.Assert(result.Count == _testObjects.ToList().Count);
    //}
}