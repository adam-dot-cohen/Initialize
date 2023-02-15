using System.Reflection;
using Initialize.Generators;

namespace Initialize.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test()
    {
        var test = new Test();
        var test2 = new Test2();
        
        //Initializer<Test>.Template.Clear();

        //Initializer<Test>.Template.Add(typeof(string),
        //    (obj, propInfo) => "string.Empty");
        //Initializer<Test>.Template.Add(typeof(Nullable<>),
        //    (obj, propInfo) => string.Format("{0}.{1}!.InitValueOrDefault()", obj, propInfo.Name));
        //Initializer<Test>.Template.Add(typeof(ValueType),
        //    (obj, propInfo) => string.Format("{0}.{1}.InitValueOrDefault()", obj, propInfo.Name));
		
        // 2. Call initialize
        Initializer<Test>.Initialize(test);

        var initializeResult = !AreAnyPropertiesNull(test);
	

        //Map example
        Mapper<Test, Test2>.Map(test, test2);

        var mapResult = AllCommonPropertiesAreEqual(test, test2);
        
        Assert.True(initializeResult && mapResult);
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
            if (!prop.GetValue(obj)!.Equals(prop2!.GetValue(value)))
                return false;
        }
        return true;
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

        var str = string.Empty;

        if (text != str)
            Console.WriteLine(text);
    }

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
public class Test
{
    public int Prop { get; set; } 
    public int? PropNullable { get; set; }
    public string PropString { get; set; }
    public int? FieldNullable { get; set; }
}