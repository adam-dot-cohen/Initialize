using System.CodeDom;
using Microsoft.CSharp;
using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Initialize;

public class MapperTemplate<TFrom, TTo>
{
    public const string Syntax = "objTo.{0} = objFrom.{1};";
    public const string SyntaxComplex = "objTo.{0} = Mapper<{2}, {2}>.Map(objFrom.{1});";
    public const string SyntaxValueType = "objTo = objFrom;";
    public const string SyntaxComplexArray = "if(objFrom?.{1} != null){{objTo.{0} = new {2}[objFrom.{1}?.Length ?? 0];for(int i = 0; i < objFrom.{1}?.Length; i++) objTo.{0}[i] = Mapper<{2}, {2}>.Map(objFrom.{1}[i]);}}";
    public const string SyntaxComplexCollection = "if(objFrom?.{1} != null){{objTo.{0} = new();foreach (var item in objFrom.{1} ?? new()) objTo.{0}.Add({2});}}";
    public const string SyntaxComplexCollectionValue = "item";
    public const string SyntaxDictionary = "if(objFrom?.{1}!=null){{objTo.{0} = new(); foreach (var item in objFrom.{1} ?? new()) objTo.{0}.Add({2}, {3});}}";
    public const string SyntaxDictionaryKey = "item.Key";
    public const string SyntaxDictionaryValue = "item.Value";
    public const string SyntaxComplexAddBody = "Mapper<{1}, {1}>.Map({0})";


    public const string ObjParam = "objTo";

    public string Generate()
    {
        using var provider = new CSharpCodeProvider();
        var type = typeof(TFrom);
        var typeTo = typeof(TTo);
        var sb = new StringBuilder();
        sb.Append($"using System;using System.Collections;using System.Collections.Generic;using System.Linq;using Initialize;{(type.Namespace != null ? "using " + type.Namespace + ";" : string.Empty)}");
        sb.Append($"namespace Mapper{type.Name}{{");
        sb.Append($"public static class Map{type.Name}{{");
        sb.Append($"public static void Map({type.FullName.Replace("+", ".")} objFrom, {typeTo.FullName.Replace("+", ".")} objTo){{");

        var propsFrom = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty).ToDictionary(x => x.Name);
        
        if (type.IsValueType && propsFrom.All(r => r.Value.PropertyType.IsValueType))
            sb.Append(SyntaxValueType);
        else
            foreach (var propTo in typeTo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                if (propsFrom.TryGetValue(propTo.Name, out var propFrom) && propFrom.PropertyType.IsAssignableTo(propTo.PropertyType))
                {
                    var ptype = Nullable.GetUnderlyingType(propFrom.PropertyType) ?? propFrom.PropertyType;

                    var friendlyTypeName = SyntaxFactory
                        .ParseName(provider.GetTypeOutput(new CodeTypeReference(ptype)))
                        .NormalizeWhitespace().ToFullString();

                    if (ptype.IsValueType || ptype == typeof(string) || ptype is { IsArray: true, UnderlyingSystemType.IsValueType: true })
                        sb.Append(string.Format(Syntax, propTo.Name, propFrom.Name));
                    else if (!ptype.IsAssignableTo(typeof(IEnumerable)))
                        sb.Append(string.Format(SyntaxComplex, propTo.Name, propFrom.Name, friendlyTypeName));
                    else if (ptype.IsAssignableTo(typeof(IDictionary)))
                    {
                        Type[] types = ptype.GetGenericArguments();
                        Type keyType = types[0];
                        Type valueType = types[1];

                        var keyTypeName = SyntaxFactory
                            .ParseName(provider.GetTypeOutput(new CodeTypeReference(keyType)))
                            .NormalizeWhitespace().ToFullString();
                        var valueTypeName = SyntaxFactory
                            .ParseName(provider.GetTypeOutput(new CodeTypeReference(valueType)))
                            .NormalizeWhitespace().ToFullString();
                        
                        var keySyntax = keyType.IsValueType || keyType == typeof(string) ? SyntaxDictionaryKey : string.Format(SyntaxComplexAddBody, SyntaxDictionaryKey, keyTypeName);
                        var valueSyntax = valueType.IsValueType ? SyntaxDictionaryValue : string.Format(SyntaxComplexAddBody, SyntaxDictionaryValue, valueTypeName);

                        sb.Append(string.Format(SyntaxDictionary, propTo.Name, propFrom.Name, keySyntax, valueSyntax));
                    }
                    else if ((propFrom.PropertyType.IsAssignableTo(typeof(ICollection)) || ptype.GetGenericTypeDefinition().IsAssignableTo((typeof(HashSet<>)))) && !propFrom.PropertyType.IsArray)
                    {
                        Type[] types = ptype.GetGenericArguments();
                        Type valueType = types[0];
                        var valueTypeName = SyntaxFactory
                            .ParseName(provider.GetTypeOutput(new CodeTypeReference(valueType)))
                            .NormalizeWhitespace().ToFullString();

                        var valueSyntax = valueType.IsValueType ? SyntaxComplexCollectionValue : string.Format(SyntaxComplexAddBody, SyntaxComplexCollectionValue, valueTypeName);

                        var str = string.Format(SyntaxComplexCollection, propTo.Name, propFrom.Name, valueSyntax);
                        sb.Append(str);
                    }
                    else if (propFrom.PropertyType.IsArray)
                    {;
                        var valueTypeName = SyntaxFactory
                            .ParseName(provider.GetTypeOutput(new CodeTypeReference(ptype)))
                            .NormalizeWhitespace().ToFullString().Replace("[ ]", string.Empty);

                        var str = string.Format(SyntaxComplexArray, propTo.Name, propFrom.Name,
                            valueTypeName);
                        sb.Append(str);
                    }
                }
            }
        sb.Append($"}}");
        sb.Append($"}}}}");
        return sb.ToString();
    }
}