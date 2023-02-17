using System;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Unicode;
using Initialize.Generators;
using NUnit.Framework.Constraints;

namespace Initialize.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    //[Test]
    //public void Test()
    //{
    //    var test = new Test();
    //    var test2 = new Test2();

    //    // 1. Optional - Customize default configuration
    //    //Initializer<Test>.DefaultTemplate.Clear();

    //    //Initializer<Test>.DefaultTemplate.Add(typeof(string), (obj, propInfo, friendlyTypeName) => "string.Empty");
    //    //Initializer<Test>.DefaultTemplate.Add(typeof(ValueType), (obj, propInfo, friendlyTypeName) => "default" );
    //    //Initializer<Test>.DefaultTemplate.Add(typeof(Nullable<>),
    //    //    (obj, propInfo, friendlyTypeName) => 
    //    //        string.Format("(({2}?){0}.{1})!.GetValueOrDefault()", obj, propInfo.Name, friendlyTypeName ) );

    //    // 2. Call initialize
    //    Initializer<Test>.Initialize(test);

    //    var initializeResult = !AreAnyPropertiesNull(test);


    //    //Map example
    //    Mapper<Test, Test2>.Map(test, test2);

    //    var mapResult = AllCommonPropertiesAreEqual(test, test2);

    //    Assert.True(initializeResult && mapResult);
    //}

    //[Test]
    //public void Should_equal_count_when_list_is_less_than_batch_size()
    //{
    //    var items = Enumerable.Range(0, 1).Select(i => new Test { Prop = i, PropString = i.ToString() }).ToList();

    //    var result = Mapper<Test, Test2>.Map(items);

    //    Assert.AreEqual(items.Count(), result.Count());
    //}

    //[Test]
    //public void Should_equal_count_when_list_is_greater_than_batch_size()
    //{
    //    var items = Enumerable.Range(0, 101).Select(i => new Test { Prop = i, PropString = i.ToString() }).ToList();

    //    var result = Mapper<Test, Test2>.Map(items);

    //    Assert.AreEqual(items.Count, result.Count());
    //}
    //[Test]
    //public void Should_equal_count_when_list_is_larg()
    //{
    //    var items = Enumerable.Range(0, 100000).Select(i => new Test { Prop = i, PropString = i.ToString() }).ToList();

    //    var result = Mapper<Test, Test2>.Map2(items);

    //    Assert.That(result.Count(), Is.EqualTo(items.Count));
    //}
    [Test]
    public void Should_equal_with_manual_config()
    {

        MapperConfiguration<Test, Test3>
            .Configure(x => x
                .For(tLeft => tLeft.PropString, tRight => tRight.PropString)
                .For(tLeft => tLeft.FieldNullable, tRight => tRight.FieldNullable)
            );

        var obj = new Test();
        obj.PropString = "1";

        var result = Mapper<Test, Test2>.Map(obj);

        Assert.That(result.PropString, Is.EqualTo(obj.PropString));
    }

    [Test]
    public void Should_equal_count_when_collections_are_not_null()
    {
        var objFrom = new Test3();
        var multidimensionalArray = new int[1][] { new int[] { 1 } };
        objFrom.ValueTypeArray = new int[1] { 1 };
        objFrom.Array = new Test3[1] { new Test3 { Prop = 1, PropString = "1" } };
        objFrom.Array2 = new Test2[] { new Test2 { Prop = 1, PropString = "1" } };
        objFrom.ValueTypeList = new List<int> { };
        objFrom.List = new List<Test3>() { new Test3 { Prop = 1, PropString = "1" } };
        objFrom.ValueTypeHashSet = new HashSet<int> { };
        objFrom.HashSet = new HashSet<Test3>() { new Test3 { Prop = 1, PropString = "1" } };
        objFrom.ValueTypeDictionary = new Dictionary<int, int> { { 1, 1 } };
        objFrom.Dictionary = new Dictionary<string, Test3> { { "1", new Test3 { Prop = 1, PropString = "1" } } };

        var objTo = Mapper<Test3, Test3>.Map(objFrom);

        Assert.AreEqual(objFrom.ValueTypeArray?.Length, objTo.ValueTypeArray?.Length);
        Assert.AreEqual(objFrom.Array?.Length, objTo.Array?.Length);
        Assert.AreEqual(objFrom.Array2?.Length, objTo.Array2?.Length);
        Assert.AreEqual(objFrom.ValueTypeList?.Count, objTo.ValueTypeList?.Count);
        Assert.AreEqual(objFrom.List?.Count, objTo.List?.Count);
        Assert.AreEqual(objFrom.ValueTypeDictionary?.Count, objTo.ValueTypeDictionary?.Count);
        Assert.AreEqual(objFrom.Dictionary?.Count, objTo.Dictionary?.Count);
        //        Assert.AreEqual(objFrom.ValueTypeHashSet?.Count, objTo.ValueTypeHashSet?.Count);
        Assert.AreEqual(objFrom.HashSet?.Count, objTo.HashSet?.Count);
    }

    [Test]
    public void Should_equal_with_manual_config_with_indexer()
    {
        Memory<byte>[] bytes = { 
            Encoding.UTF8.GetBytes(DateTime.Now.ToString()), 
            "8"u8.ToArray(), 
            ""u8.ToArray(),  //skip
            "1.0"u8.ToArray(), 
            "hello"u8.ToArray() };

        int indexOffset = 0;
        MapperConfiguration<Memory<byte>[], ParseTest>
            .Configure(x => x
                // index 0
                .For(t => t.PropDateTimeNullable,
                    "DateTime.Parse(Encoding.UTF8.GetString( {0} [{1}].Span))", 0)
                // index 1
                .For(t => t.Prop,
                    "int.Parse(Encoding.UTF8.GetString( {0} [{1}].Span))", 0)
                // index 3 - skipping index 2
                .For(t => t.PropDouble,
                    "double.Parse(Encoding.UTF8.GetString( {0} [{1}].Span))", ++indexOffset)
                // index 4
                .For(t => t.PropString,
                    "Encoding.UTF8.GetString( {0} [{1}].Span)", indexOffset)
            );

        var result = Mapper<Memory<byte>[], ParseTest>.Map(bytes);

        // equality values
        var equalToDt = DateTime.Parse(Encoding.UTF8.GetString(bytes[0].Span));
        var equalToInt = int.Parse(Encoding.UTF8.GetString("8"u8.ToArray()));
        var equalToDouble = double.Parse(Encoding.UTF8.GetString(bytes[3].Span));
        var equalToString = Encoding.UTF8.GetString(bytes[4].Span);

        // equality tests
        Assert.That(result.PropDateTimeNullable, Is.EqualTo(equalToDt));
        Assert.That(result.Prop, Is.EqualTo(equalToInt));
        Assert.That(result.PropDouble, Is.EqualTo(equalToDouble));
        Assert.That(result.PropString, Is.EqualTo(equalToString));
    }
    [Test]
    public void Should_equal_with_manual_config_with_indexer_from_ParseUT8()
    {
        var dtBytes = Encoding.UTF8.GetBytes(DateTime.Now.ToString());
        Memory<byte>[] bytes = { dtBytes, "8"u8.ToArray(), ""u8.ToArray(), "1.0"u8.ToArray(), "hello"u8.ToArray() };

        int indexOffset = 0;
        MapperConfiguration<Memory<byte>[], ParseTest>
            .Configure(x => x
                // index 0
                .ParseFor(t => t.PropDateTimeNullable,
                    parse => parse.MemoryByteToDateTimeNullable)
                // index 1
                .ParseFor(t => t.Prop,
                    parse => parse.MemoryByteToInt)
                // index 3 - skipping index 2
                .ParseFor(t => t.PropDouble,
                        parse => parse.MemoryByteToDoubleNullable, ++indexOffset)
                // index 4
                .ParseFor(t => t.PropString,
                        parse => parse.MemoryByteToStringNullIfEmpty, indexOffset)
            );

        var result = Mapper<Memory<byte>[], ParseTest>.Map(bytes);

        // equality values
        var equalToDt = DateTime.Parse(Encoding.UTF8.GetString(bytes[0].Span));
        var equalToInt = int.Parse(Encoding.UTF8.GetString("8"u8.ToArray()));
        var equalToDouble = double.Parse(Encoding.UTF8.GetString(bytes[3].Span));
        var equalToString = Encoding.UTF8.GetString(bytes[4].Span);

        // equality tests
        Assert.That(result.PropDateTimeNullable, Is.EqualTo(equalToDt));
        Assert.That(result.Prop, Is.EqualTo(equalToInt));
        Assert.That(result.PropDouble, Is.EqualTo(equalToDouble));
        Assert.That(result.PropString, Is.EqualTo(equalToString));
    }
    [Test]
    public void GenerateDtoWithoutNotifyPropertyChanged()
    {
        var text = CSharpGeneratorFactory.GenerateDto<Test>("Tester");

        Assert.AreEqual(text, generatedDto);
    }

    [Test]
    public void GenerateDtoWithNotifyPropertyChanged()
    {
        var text = CSharpGeneratorFactory.GenerateDtoWithNotifyPropertyChanged<Test>("Tester");

        Assert.AreEqual(text, generatedDtoWithPropertyChanged);
    }

    private bool AreAnyPropertiesNull<TObject>(TObject obj, params string[]? exclude)
    {
        foreach (var prop in typeof(TObject).GetProperties(BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty)
                     .Where(g => exclude == null || exclude.All(q => q != g.Name)))
        {
            if (prop.GetValue(obj)!.Equals(null))
                return true;
        }
        return false;
    }
    private bool AllCommonPropertiesAreEqual<TObject, TObject2>(TObject obj, TObject2 value, params string[]? exclude)
    {
        foreach (var prop in typeof(TObject).GetProperties()
                     .Where(g => typeof(TObject2).GetProperties().Any(h => h.Name == g.Name) && (exclude == null || exclude.All(q => q != g.Name))))
        {
            var prop2 = typeof(TObject2).GetProperty(prop.Name);
            var val1 = prop.GetValue(obj);
            var val2 = prop2.GetValue(value);
            if (val1 is null && val2 is null || val1.Equals(val1))
                return false;
        }
        return true;
    }

    private const string generatedDtoWithPropertyChanged =
        "using System;\r\nusing System.ComponentModel;\r\nusing System.Runtime.CompilerServices;\r\nusing System.Runtime.Serialization;\r\n\r\nnamespace Tester;\r\n[DataContract]\r\npublic class TestDto : INotifyPropertyChanged\r\n{\r\n    public TestDto()\r\n    {\r\n    }\r\n\r\n    public TestDto(Initialize.Tests.Test obj)\r\n    {\r\n        _prop = obj.Prop;\r\n        _propNullable = obj.PropNullable;\r\n        _propString = obj.PropString;\r\n        _fieldNullable = obj.FieldNullable;\r\n    }\r\n\r\n    private int _prop;\r\n    [DataMember(Order = 0)]\r\n    public int Prop\r\n    {\r\n        get => _prop;\r\n        set\r\n        {\r\n            _prop = value;\r\n            RaisePropertyChanged();\r\n        }\r\n    }\r\n\r\n    private System.Nullable<int> _propNullable;\r\n    [DataMember(Order = 1)]\r\n    public System.Nullable<int> PropNullable\r\n    {\r\n        get => _propNullable;\r\n        set\r\n        {\r\n            _propNullable = value;\r\n            RaisePropertyChanged();\r\n        }\r\n    }\r\n\r\n    private string _propString;\r\n    [DataMember(Order = 2)]\r\n    public string PropString\r\n    {\r\n        get => _propString;\r\n        set\r\n        {\r\n            _propString = value;\r\n            RaisePropertyChanged();\r\n        }\r\n    }\r\n\r\n    private System.Nullable<int> _fieldNullable;\r\n    [DataMember(Order = 3)]\r\n    public System.Nullable<int> FieldNullable\r\n    {\r\n        get => _fieldNullable;\r\n        set\r\n        {\r\n            _fieldNullable = value;\r\n            RaisePropertyChanged();\r\n        }\r\n    }\r\n\r\n    public event PropertyChangedEventHandler PropertyChanged;\r\n    public void RaisePropertyChanged([CallerMemberName] string propertyName = null)\r\n    {\r\n        if (PropertyChanged != null)\r\n        {\r\n            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));\r\n        }\r\n    }\r\n}";
    private const string generatedDto =
        "using System;\r\nusing System.ComponentModel;\r\nusing System.Runtime.CompilerServices;\r\nusing System.Runtime.Serialization;\r\n\r\nnamespace Tester;\r\n[DataContract]\r\npublic class TestDto\r\n{\r\n    public TestDto()\r\n    {\r\n    }\r\n\r\n    public TestDto(Initialize.Tests.Test obj)\r\n    {\r\n        _prop = obj.Prop;\r\n        _propNullable = obj.PropNullable;\r\n        _propString = obj.PropString;\r\n        _fieldNullable = obj.FieldNullable;\r\n    }\r\n\r\n    private int _prop;\r\n    [DataMember(Order = 0)]\r\n    public int Prop\r\n    {\r\n        get => _prop;\r\n        set\r\n        {\r\n            _prop = value;\r\n        }\r\n    }\r\n\r\n    private System.Nullable<int> _propNullable;\r\n    [DataMember(Order = 1)]\r\n    public System.Nullable<int> PropNullable\r\n    {\r\n        get => _propNullable;\r\n        set\r\n        {\r\n            _propNullable = value;\r\n        }\r\n    }\r\n\r\n    private string _propString;\r\n    [DataMember(Order = 2)]\r\n    public string PropString\r\n    {\r\n        get => _propString;\r\n        set\r\n        {\r\n            _propString = value;\r\n        }\r\n    }\r\n\r\n    private System.Nullable<int> _fieldNullable;\r\n    [DataMember(Order = 3)]\r\n    public System.Nullable<int> FieldNullable\r\n    {\r\n        get => _fieldNullable;\r\n        set\r\n        {\r\n            _fieldNullable = value;\r\n        }\r\n    }\r\n}";
}

public class Test2
{
    public int Prop { get; set; }
    public int? PropNullable { get; set; }
    public string PropString { get; set; }
    public int? FieldNullable { get; set; }
}
public class ParseTest
{
    public int Prop { get; set; }
    public int? PropNullable { get; set; }
    public string PropString { get; set; }
    public DateTime? PropDateTimeNullable { get; set; }
    public double? PropDouble { get; set; }
}
public class Test
{
    public int Prop { get; set; }
    public int? PropNullable { get; set; }
    public string PropString { get; set; }
    public int? FieldNullable { get; set; }
}

public class Test3
{
    public int Prop { get; set; }
    public int? PropNullable { get; set; }
    public string PropString { get; set; }
    public int? FieldNullable { get; set; }
    public int[] ValueTypeArray { get; set; }
    public Test3[] Array { get; set; }
    public Test2[] Array2 { get; set; }
    public List<int> ValueTypeList { get; set; }
    public List<Test3> List { get; set; }
    //public ImmutableArray<Test3> ImmutableArray { get; set; }
    //public IReadOnlyCollection<Test3> ReadOnlyCollection { get; set; }
    public Dictionary<int, int> ValueTypeDictionary { get; set; }
    public Dictionary<string, Test3> Dictionary { get; set; }
    public HashSet<int> ValueTypeHashSet { get; set; }
    public HashSet<Test3> HashSet { get; set; }
}