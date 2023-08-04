using System.Collections;
using System.Reflection;
using System.Text;

namespace Initialize.Mapper;

public class MapperDefaultTemplate<TFrom, TTo> : MapperTemplateBase<TFrom, TTo>
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
    

    protected override void GenerateBody(out StringBuilder syntaxBuilder)
    {
        var type = typeof(TFrom);
        var typeTo = typeof(TTo);

        syntaxBuilder = new StringBuilder();

        var propsFrom = type
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty |
                           BindingFlags.SetProperty).ToDictionary(x => x.Name);

        if (type.IsValueType && propsFrom.All(r => r.Value.PropertyType.IsValueType))
            syntaxBuilder.Append(SyntaxValueType);
        else
            foreach (var propTo in typeTo.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                        BindingFlags.GetProperty | BindingFlags.SetProperty))
            {
                if (propsFrom.TryGetValue(propTo.Name, out var propFrom) &&
                    propFrom.PropertyType.IsAssignableTo(propTo.PropertyType))
                {
                    var ptype = Nullable.GetUnderlyingType(propFrom.PropertyType) ?? propFrom.PropertyType;

                    if (ptype.IsValueType || ptype == typeof(string) || ptype is
                            { IsArray: true, UnderlyingSystemType.IsValueType: true })
                        syntaxBuilder.Append(string.Format(Syntax, propTo.Name, propFrom.Name).AsSpan());
                    else if (!ptype.IsAssignableTo(typeof(IEnumerable)))
                        syntaxBuilder.Append(string.Format(SyntaxComplex, propTo.Name, propFrom.Name,
                            GetTypeSyntax(ptype)).AsSpan());
                    else if (ptype.IsAssignableTo(typeof(IDictionary)))
                    {
                        Type[] types = ptype.GetGenericArguments();
                        Type keyType = types[0];
                        Type valueType = types[1];

                        var keySyntax = keyType.IsValueType || keyType == typeof(string)
                            ? SyntaxDictionaryKey
                            : string.Format(SyntaxComplexAddBody, SyntaxDictionaryKey, GetTypeSyntax(keyType));
                        var valueSyntax = valueType.IsValueType
                            ? SyntaxDictionaryValue
                            : string.Format(SyntaxComplexAddBody, SyntaxDictionaryValue, GetTypeSyntax(valueType));

                        syntaxBuilder.Append(string.Format(SyntaxDictionary, propTo.Name, propFrom.Name, keySyntax, valueSyntax).AsSpan());
                    }
                    else if ((propFrom.PropertyType.IsAssignableTo(typeof(ICollection)) ||
                              ptype.GetGenericTypeDefinition().IsAssignableTo((typeof(HashSet<>)))) &&
                             !propFrom.PropertyType.IsArray)
                    {
                        Type[] types = ptype.GetGenericArguments();
                        Type valueType = types[0];

                        var valueSyntax = valueType.IsValueType
                            ? SyntaxComplexCollectionValue
                            : string.Format(SyntaxComplexAddBody, SyntaxComplexCollectionValue,
                                GetTypeSyntax(valueType));

                        var str = string.Format(SyntaxComplexCollection, propTo.Name, propFrom.Name, valueSyntax);
                        syntaxBuilder.Append(str.AsSpan());
                    }
                    else if (propFrom.PropertyType.IsArray)
                    {
                        var str = string.Format(SyntaxComplexArray, propTo.Name, propFrom.Name,
                            GetTypeSyntax(ptype).Replace("[ ]", string.Empty).Replace("[]", string.Empty));
                        syntaxBuilder.Append(str.AsSpan());
                    }
                }
            }
    }
}