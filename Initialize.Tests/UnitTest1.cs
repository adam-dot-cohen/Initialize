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
        
        Initializer<Test>.Template.Clear();

        Initializer<Test>.Template.Add(typeof(string),
            (obj, propInfo) => "string.Empty");
        Initializer<Test>.Template.Add(typeof(Nullable<>),
            (obj, propInfo) => string.Format("{0}.{1}!.InitValueOrDefault()", obj, propInfo.Name));
        Initializer<Test>.Template.Add(typeof(ValueType),
            (obj, propInfo) => string.Format("{0}.{1}.InitValueOrDefault()", obj, propInfo.Name));
		
        // 2. Call initialize
        Initializer<Test>.Initialize(test);
	

        //Map example
        Mapper<Test, Test2>.Map(test, test2);

        Assert.True(true);
    }


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