# Overview
![]()
[![NuGet version (Initializer-Alpha)](https://img.shields.io/badge/nuget-v0.0.1-blue?style=flat-square)](https://www.nuget.org/packages/Initializer/)

High performance automatic DTO mapper and properties initializer based on Roslyn dynamic code and compilation

# Example Usage
```csharp
	var test = new Test();
	var test2 = new Test2();

	// INITIALIZER EXAMPLE
	test.Dump("Test Pre Init");

	// 1.Optional If you want to manipuate the default initialization logic.
	Initializer<Test>.Template.Clear();

	Initializer<Test>.Template.Add(typeof(string),
		(obj, propInfo) => "string.Empty");
	Initializer<Test>.Template.Add(typeof(Nullable<>),
		(obj, propInfo) => string.Format("{0}.{1}!.InitValueOrDefault()", obj, propInfo.Name));
	Initializer<Test>.Template.Add(typeof(ValueType),
		(obj, propInfo) => string.Format("{0}.{1}.InitValueOrDefault()", obj, propInfo.Name));
		
	// 2. Call initialize 
	Initializer<Test>.Initialize(test);
	test.Dump("Test Post Init");
	

	//MAPPER EXAMPLE
	test2.Dump("Test2 Pre Map");
	Mapper<Test, Test2>.Map(test, test2);
	test2.Dump("Test2 Post Map");
```

## Mapper Benchmarks   
```
BenchmarkDotNet=v0.13.4, OS=Windows 11 (10.0.22621.1265)
Intel Core i9-10980XE CPU 3.00GHz, 1 CPU, 36 logical and 18 physical cores
.NET SDK=7.0.103
  [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2
  Job-DVEUFE : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX2

Jit=RyuJit  Runtime=.NET 7.0  Arguments=/p:Optimize=true
InvocationCount=1  LaunchCount=1  RunStrategy=Throughput
UnrollFactor=1

|           Method |     Mean |     Error | Allocated |
|----------------- |---------:|----------:|----------:|
| InitializeMapper | 2.178 us | 0.1041 us |     880 B |
|       AutoMapper | 5.857 us | 0.3292 us |     880 B |
```

## Feedback, Suggestions and Contributions
Are all welcome!
