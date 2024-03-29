﻿using System.CodeDom;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;

namespace Initialize.Initialize;

public class InitializerTemplate<T>
{
    public const string Syntax = "obj.{0} = {1};";

    public const string ObjParam = "obj";

    private Dictionary<Type, Func<string, PropertyInfo, string, string>> SyntaxForPropertyType => new() {
        { typeof(string), (obj, propInfo, typeName) => "string.Empty"},
        { typeof(Nullable<>), (obj, propInfo, typeName) => string.Format("(({2}?){0}.{1})!.GetValueOrDefault()", obj, propInfo.Name, typeName ) },
        { typeof(ValueType), (obj, propInfo, typeName) =>"default" }
    };
    private Dictionary<string, string> SyntaxForPropertyName { get; } = new();

    #region Override Default Initialization
    public void Add(Type propertyType, string rightAssignment)
    {
        if (rightAssignment[rightAssignment.Length] == ';') rightAssignment = rightAssignment[..^1];
        if(!this.SyntaxForPropertyType.TryAdd(propertyType, (obj, prop, typeName) => rightAssignment))
			this.SyntaxForPropertyType[propertyType] = (obj, prop, typeName) => rightAssignment;
    }

    public void Add(Type propertyType, Func<string, PropertyInfo, string, string> rightAssignment)
    {
        if (!this.SyntaxForPropertyType.TryAdd(propertyType, rightAssignment))
			this.SyntaxForPropertyType[propertyType] = rightAssignment;
    }

    public void Clear() 
    {
		this.SyntaxForPropertyType.Clear();
		this.SyntaxForPropertyName.Clear();
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
        using var provider = new CSharpCodeProvider();
        var type = typeof(T);
        var sb = new StringBuilder();
        sb.Append($"using System;using System.Collections;using System.Collections.Generic;using System.Linq;{(type.Namespace != null ? "using " + type.Namespace + ";" : string.Empty)}");
        sb.Append($"namespace Initializer{{");
        sb.Append($"public static class Initializer_{type.Name}{{");
        sb.Append($"public static void Initialize({type.FullName.Replace("+", ".")} obj){{");

        //var valueType = SyntaxForPropertyType.First(x => x.Key == typeof(ValueType));
        //var nullableType = SyntaxForPropertyType
        Span<PropertyInfo> properties =  type.GetProperties();

        foreach (var prop in properties)////.Where(g => g.PropertyType == typeof(TValue) && (exclude == null || !exclude.Any(q => q == g.Name))))
        {
            //var ptype = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            
            //var friendlyTypeName = SyntaxFactory
            //    .ParseName(provider.GetTypeOutput(new CodeTypeReference(ptype)))
            //    .NormalizeWhitespace().ToFullString();

            if (this.SyntaxForPropertyName.TryGetValue(prop.Name, out var nameFunc))
            {
                sb.Append(string.Format(Syntax, prop.Name, this.SyntaxForPropertyName));
                continue;
            }

            var result = this.SyntaxForPropertyType.FirstOrDefault(x => x.Key == prop.PropertyType);
            if (result!.Key != null)
            {
                sb.Append(string.Format(Syntax, prop.Name, result.Value(ObjParam, prop, null)));
                continue;
            }

            result = this.SyntaxForPropertyType.FirstOrDefault(x => x.Key == typeof(Nullable<>));
            if (Nullable.GetUnderlyingType(prop.PropertyType) != null && result!.Key != null)
            {

                var ptype = Nullable.GetUnderlyingType(prop.PropertyType);
                var friendlyTypeName = SyntaxFactory
                    .ParseName(provider.GetTypeOutput(new CodeTypeReference(ptype)))
                    .NormalizeWhitespace().ToFullString();
                sb.Append(string.Format(Syntax, prop.Name, result.Value(ObjParam, prop, friendlyTypeName)));
                continue;
            }

            result = this.SyntaxForPropertyType.FirstOrDefault(x => x.Key == typeof(ValueType));
            if (Nullable.GetUnderlyingType(prop.PropertyType) == null && prop.PropertyType.IsValueType
                                                                      && result!.Key != null)
            {
                sb.Append(string.Format(Syntax, prop.Name, result.Value(ObjParam, prop, null)));
                continue;
            }
        }
        sb.Append($"}}");
        sb.Append($"}}}}");

        return sb.ToString();
    }
}