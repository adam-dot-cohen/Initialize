# Overview
![]()
[![NuGet version (Initialize)](https://img.shields.io/badge/nuget-v0.2.6-blue?style=flat-square)](https://www.nuget.org/packages/Initialize/)

High performance automatic object-to-object mapper for zero configuration property binding.

# Example Usage
```csharp
	var test = new Test();
	var test2 = new Test2();

	//MAPPER EXAMPLE
	test2.Dump("Test2 Pre Map");
	Mapper<Test, Test2>.Map(test, test2);
	test2.Dump("Test2 Post Map");
```

## Mapper Benchmarks   
Initializer's Mapper is roughly 4x faster than AutoMapper...
```
BenchmarkDotNet=v0.13.4, OS=Windows 11 (10.0.22621.1265)
Intel Core i9-10980XE CPU 3.00GHz, 1 CPU, 36 logical and 18 physical cores
.NET SDK=7.0.103
  [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
  Job-CCLJAY : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2

Jit=RyuJit  Runtime=.NET 7.0  Arguments=/p:Optimize=true
InvocationCount=1  LaunchCount=1  RunStrategy=Throughput
UnrollFactor=1

|           Method |     Mean |     Error | Allocated |
|----------------- |---------:|----------:|----------:|
| InitializeMapper | 2.204 us | 0.1209 us |     880 B |
|       AutoMapper | 7.990 us | 0.6964 us |     880 B |
```

## Feedback, Suggestions and Contributions
Are all welcome!
