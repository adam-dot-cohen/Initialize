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
   
## Feedback, Suggestions and Contributions
Are all welcome!
