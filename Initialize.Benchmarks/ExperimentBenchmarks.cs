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
using Microsoft.Diagnostics.Runtime.Utilities;
using System.Buffers;

namespace Initialize.Benchmarks;

[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1,
    runtimeMoniker: RuntimeMoniker.Net70)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
[HideColumns(Column.StdDev, Column.Median)]
public class ExperimentBenchmarks
{
    private MapperA _autoMapper;
    private IEnumerable<Test> _testEnumerable;

    public ExperimentBenchmarks()
    {
        //Initialize mapper
        var config = new MapperConfiguration(cfg =>
            cfg.CreateMap<Test, Test2>().ForMember(x=>
                    x.PropString, 
                x=>x.MapFrom(x=>x.PropString)));
        _autoMapper = new MapperA(config);

        //Initialize mapper
        Mapper<Test, Test2>.Map(testObj);
        
    }
    [Params(100_000, 1_000_000)]
    public int Iterations { get; set; }

    [IterationSetup]
    public void Setup()
    {
        _testEnumerable = Enumerable.Range(0, Iterations)
            .Select(r => new Test() { PropString = r.ToString(), Prop = r });
        
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

    [Benchmark(Baseline = true)]
    public void MapArray()
    {
        var test2 = new Test2();

        var result = Mapper<Test, Test2>.Map(_testEnumerable);

        Debug.Assert(result.Count() == _testEnumerable.Count());
    }
    //[Benchmark]
    //public void Span()
    //{
    //    var test2 = new Test2();
    //    int cnt = 0, current = 0;
    //    var span = _testObjects.ToArray().AsSpan();
    //    Span<Test2> spanTo = new Test2[span.Length];

    //    for (int i = 0; i < cnt; i++)
    //        spanTo[i] =  Mapper<Test, Test2>.Map(span[i]);

       
    //    Debug.Assert(spanTo.ToArray().Count() == _testObjects.Count());
    //}
    [Benchmark]
    public void MapInline()
    {
        var test2 = new Test2();

        var result = Mapper<Test, Test2>.Map(_testEnumerable);

        Debug.Assert(result.Count() == _testEnumerable.Count());
    }    
    [Benchmark]
    public void MapOptimized()
    {
        var test2 = new Test2();

        var result = Mapper<Test, Test2>.Map(_testEnumerable);

        Debug.Assert(result.Count() == _testEnumerable.Count());
    }

    [Benchmark]
    public void ArrayPool()
    {
        var test2 = new Test2();
        int cnt = 0, current = 0;
        var span = _testEnumerable.ToArray();
        Test2[] spanTo = ArrayPool<Test2>.Shared.Rent(span.Length);

        for (int i = 0; i < cnt; i++)
            spanTo[i] =  Mapper<Test, Test2>.Map(span[i]);
        
        ArrayPool<Test2>.Shared.Return(spanTo);
        Debug.Assert(spanTo.Count() == _testEnumerable.Count());
    }
}