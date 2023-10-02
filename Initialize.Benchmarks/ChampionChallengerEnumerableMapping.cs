﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        this._autoMapper = new AutoMapper.Mapper(config);

        //Initialize mapper
        Mapper<Test, Test2>.Map(new Test());
    }

    [Params(100, 1_000, 10_000, 100_000, 1_000_000)]
    public int Iterations { get; set; }

    [IterationSetup]
    public void Setup()
    {
        this._testEnumerable = Enumerable.Range(0, this.Iterations)
            .Select(r => new Test() { PropString = r.ToString(), Prop = r });

       this._testObjects = this._testEnumerable.ToList();
    }

    [Benchmark(Description = "AutoMapper")]
    public void AutoMapperEnumerable()
    {
        var result = this._autoMapper.Map<IEnumerable<Test>>(_testEnumerable);
        
       // if ((result.TryGetNonEnumeratedCount(out var cnt) && cnt != Iterations) || result.Count() != Iterations) throw new Exception($"{result.Count()} and {Iterations}not equal");
    }

    [Benchmark(Description = "Initialize")]
    public void MapperEnumerable()
    {
        var result = Mapper<Test, Test2>.Map(_testEnumerable);
        
        //if ((result.TryGetNonEnumeratedCount(out var cnt) && cnt != Iterations) || result.Count() != Iterations) throw new Exception($"{result.Count()} and {Iterations}not equal");
    }
}