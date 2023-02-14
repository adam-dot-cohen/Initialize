using System.Reflection;
using System.Text;

namespace Initialize;

public class InitializerTemplate<T>
{
    public const string Syntax = "obj.{0} = {1};";

    public const string ObjParam = "obj";

    private Dictionary<Type, Func<string, PropertyInfo, string>> SyntaxForPropertyType => new() {
        { typeof(string), (obj, propInfo) => "string.Empty"},
        { typeof(Nullable<>), (obj, propInfo) => string.Format("{0}.{1}!.InitValueOrDefault()", obj, propInfo.Name) },
        { typeof(ValueType), (obj, propInfo) => string.Format("{0}.{1}.InitValueOrDefault()", obj, propInfo.Name) }
    };

    private Dictionary<string, string> SyntaxForPropertyName { get; } = new();

    #region Override Default Initialization
    public void Add(Type propertyType, string rightAssignment)
    {
        if (rightAssignment[rightAssignment.Length] == ';') rightAssignment = rightAssignment[..^1];
        if(!SyntaxForPropertyType.TryAdd(propertyType, (obj, prop) => rightAssignment))
            SyntaxForPropertyType[propertyType] = (obj, prop) => rightAssignment;
    }

    public void Add(Type propertyType, Func<string, PropertyInfo, string> rightAssignment)
    {
        if (!SyntaxForPropertyType.TryAdd(propertyType, rightAssignment))
            SyntaxForPropertyType[propertyType] = rightAssignment;
    }

    public void Clear() 
    { 
        SyntaxForPropertyType.Clear();
        SyntaxForPropertyName.Clear();
    }
//
//	public void Add<TProperty>(Expression<Func<TProperty>> property, string rightAssignment)
//	{
//		throw new Exception("Not tested.  QA before before using");
//
//		var propName = (property.Body as MemberExpression).Member.Name;
//
//		if (string.IsNullOrWhiteSpace(propName)) throw new ArgumentException("Property is invalid");
//
//		if (rightAssignment.Last() != ';') rightAssignment = rightAssignment + ";";
//
//		SyntaxForPropertyName.Add(propName, rightAssignment);
//	}
    #endregion

    public string Generate()
    {
        var type = typeof(T);
        var sb = new StringBuilder();
        sb.Append($"using System;using System.Collections;using System.Collections.Generic;using System.Linq;using Initialize;{(type.Namespace != null ? "using " + type.Namespace + ";" : string.Empty)}");
        sb.Append($"namespace Initializer{type.Name}{{");
        sb.Append($"public static class Init{type.Name}{{");
        sb.Append($"public static void Initialize({type.FullName.Replace("+", ".")} obj){{");

        foreach (var prop in type.GetProperties())//.Where(g => g.PropertyType == typeof(TValue) && (exclude == null || !exclude.Any(q => q == g.Name))))
        {
            if (SyntaxForPropertyName.TryGetValue(prop.Name, out var nameFunc))
            {
                sb.Append(string.Format(Syntax, prop.Name, SyntaxForPropertyName));
                continue;
            }

            var result = SyntaxForPropertyType.FirstOrDefault(x => x.Key == prop.PropertyType);
            if (result!.Key != null)
            {
                sb.Append(string.Format(Syntax, prop.Name, result.Value(ObjParam, prop)));
                continue;
            }

            result = SyntaxForPropertyType.FirstOrDefault(x => x.Key == typeof(Nullable<>));
            if (Nullable.GetUnderlyingType(prop.PropertyType) != null && result!.Key != null)
            {
                sb.Append(string.Format(Syntax, prop.Name, result.Value(ObjParam, prop)));
                continue;
            }

            result = SyntaxForPropertyType.FirstOrDefault(x => x.Key == typeof(ValueType));
            if (Nullable.GetUnderlyingType(prop.PropertyType) == null && prop.PropertyType.IsValueType
                                                                      && result!.Key != null)
            {
                sb.Append(string.Format(Syntax, prop.Name, result.Value(ObjParam, prop)));
                continue;
            }
        }
        sb.Append($"}}");
        sb.Append($"}}}}");
        return sb.ToString();
    }
}