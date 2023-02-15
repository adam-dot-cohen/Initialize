using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MapperA = AutoMapper.Mapper;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Microsoft.IO;
using Microsoft.Diagnostics.Runtime.Utilities;

namespace Initialize.Benchmarks;

[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1,
    runtimeMoniker: RuntimeMoniker.Net70)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
[HideColumns(Column.StdDev, Column.Median)]
public class ChampionChallengerBenchmarks
{
    private static readonly RecyclableMemoryStreamManager manager = new();
    private MapperA _autoMapper;
    //private IEnumerable<Test> _testObjects;

    public ChampionChallengerBenchmarks()
    {
        //Initialize mapper
        var config = new MapperConfiguration(cfg =>
            cfg.CreateMap<Test, Test2>());
        _autoMapper = new MapperA(config);

        //Initialize mapper
        Mapper<Test, Test2>.Map(testObj);

        // _testObjects = Enumerable.Range(0, 100).Select(r => new Test() { PropString = r.ToString(), Prop = r });
    }

    private Test testObj = new Test()
    {
        Prop = 1,
        PropString = "a",
    };

    //[Benchmark(Description = "InitializeMapperColdStart")]
    //public void InitializeMapperColdStart()
    //{
    //    var test2 = new Test2();

    //    Mapper<Test, Test2>.Map(testObj, test2);
    //}

    //[Benchmark(Description = "AutoMapperColdStart")]
    //public void AutoMapperUnInitialized()
    //{
    //    //Initialize the mapper
    //    var config = new MapperConfiguration(cfg =>
    //        cfg.CreateMap<Test, Test2>());

    //    var mapper = new MapperA(config);

    //    var test2 = new Test2();

    //    mapper.Map(testObj, test2);
    //}

    [Benchmark(Description = "InitializeMapper")]
    public void InitializeMapper()
    {
        var test2 = new Test2();

        Mapper<Test, Test2>.Map(testObj, test2);
    }

    [Benchmark(Description = "AutoMapper")]
    public void AutoMapper()
    {
        var test2 = new Test2();

        _autoMapper.Map(testObj, test2);
    }

    //[Benchmark(Description = "InitializeMapper_100KCollection")]
    //public void InitializeMapper100K()
    //{
    //    var test2 = new Test2();

    //    var result = Mapper<Test, Test2>.Map(_testObjects);

    //    Debug.Assert(result.Count == _testObjects.Count);   
    //}
    //[Benchmark(Description = "InitializeMapper_100KCollectionFast")]
    //public void InitializeMapper100KFast()
    //{
    //    var test2 = new Test2();

    //    var result = Mapper<Test, Test2>.MapFast(_testObjects);

    //    Debug.Assert(result.Count == _testObjects.Count);
    //}
    //[Benchmark(Description = "AutoMapper_100KCollection")]
    //public void AutoMapper100K()
    //{
    //    var test2 = new Test2();

    //    var result = _autoMapper.Map<List<Test>>(_testObjects);

    //    Debug.Assert(result.Count == _testObjects.ToList().Count);
    //}
}


public class Test2
{
    public int Prop { get; set; }
    public int? PropNullable { get; set; }
    public string PropString { get; set; }
}
public class Test
{
    public int Prop { get; set; }
    public int? PropNullable { get; set; }
    public string PropString { get; set; }
}
