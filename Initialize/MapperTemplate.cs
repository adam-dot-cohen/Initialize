using System.Reflection;
using System.Text;

namespace Initialize;

public class MapperTemplate<TFrom, TTo>
{
    public const string Syntax = "objTo.{0} = objFrom.{1};";

    public const string ObjParam = "objTo";

    public string Generate()
    {
        var type = typeof(TFrom);
        var typeTo = typeof(TTo);
        var sb = new StringBuilder();
        sb.Append($"using System;using System.Collections;using System.Collections.Generic;using System.Linq;{(type.Namespace != null ? "using " + type.Namespace + ";" : string.Empty)}");
        sb.Append($"namespace Mapper{type.Name}{{");
        sb.Append($"public static class Map{type.Name}{{");
        sb.Append($"public static void Map({type.FullName.Replace("+", ".")} objFrom, {typeTo.FullName.Replace("+", ".")} objTo){{");

        var propsFrom = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty).ToDictionary(x => x.Name);

        foreach (var propTo in typeTo.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty))
        {
            if (propsFrom.TryGetValue(propTo.Name, out var propFrom)
                && propFrom.PropertyType.IsAssignableTo(propTo.PropertyType))
                sb.Append(string.Format(Syntax, propTo.Name, propFrom.Name));
        }
        sb.Append($"}}");
        sb.Append($"}}}}");
        return sb.ToString();
    }
}