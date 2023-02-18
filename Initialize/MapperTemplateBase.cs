using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Initialize;

public interface IMapperTemplate<TFrom, TTo>
{
    string FullName { get; }
    string Generate();
}

public abstract class MapperTemplateBase<TFrom, TTo> : IMapperTemplate<TFrom, TTo>
{
    CSharpCodeProvider provider = new ();
    private string _namespace;
    private string _className;
    protected  const string SyntaxVarFrom = "objFrom";
    protected const string SyntaxVarTo = "objTo";

    protected MapperTemplateBase()
    {
        _namespace =  $"Mapper.{GetTypeSyntaxWithoutSpecialCharacter(typeof(TFrom)).Trim()}{GetTypeSyntaxWithoutSpecialCharacter(typeof(TTo)).Trim()}";
        _className = $"{GetTypeSyntaxWithoutSpecialCharacter(typeof(TFrom))}_{GetTypeSyntaxWithoutSpecialCharacter(typeof(TTo))}";
    }

    public string FullName => $"{_namespace}.{_className}";
       

    public string Generate()
    {
        var type = typeof(TFrom);
        var typeTo = typeof(TTo);
        var syntaxTypeFrom = GetTypeSyntax(type);
        var syntaxTypeTo = GetTypeSyntax(typeTo);
        var namespaceFrom = GetNamespace(type);
        var namespaceTo = GetNamespace(typeTo);
        var usingFrom = string.IsNullOrWhiteSpace(namespaceFrom) ? string.Empty : $"using {namespaceFrom};";
        var usingTo = string.IsNullOrWhiteSpace(namespaceTo) ? string.Empty :$"using {namespaceTo};";
        var syntaxUsing = namespaceFrom == namespaceTo ? usingFrom : usingFrom + usingTo;
        var sb = new StringBuilder();

        GenerateBody(out var bodySyntax);

        sb.Append($"using System;using System.Collections;using System.Collections.Generic;using System.Runtime.CompilerServices;using System.Linq;using System.Text;using Initialize;{syntaxUsing}");
        
        sb.Append($"namespace {_namespace}{{");
        
            sb.Append($"public static class {_className}{{");
        
                    #region public static void Map(objFrom, objTo)
                    sb.Append("[MethodImpl(MethodImplOptions.AggressiveInlining)]");;

                    sb.Append($"public static void Map({syntaxTypeFrom} {SyntaxVarFrom}, {syntaxTypeTo} {SyntaxVarTo}){{");
                        
                        sb.Append(bodySyntax.ToString());

                    sb.Append($"}}");
                    #endregion

                    #region public static TTo Map(objFrom)
                    sb.Append("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
                    
                    sb.Append($"public static {syntaxTypeTo} Map({syntaxTypeFrom} {SyntaxVarFrom}){{");
                    
                        sb.Append($"var {SyntaxVarTo} = new {syntaxTypeTo}();");
                        
                        sb.Append(bodySyntax);
                        
                        sb.Append($"return {SyntaxVarTo};");

                    sb.Append($"}}");
                    #endregion
            
            sb.Append($"}}"); // end class

        sb.Append($"}}"); // end namespace

        return sb.ToString();
    }

    protected abstract void GenerateBody(out StringBuilder syntaxBuilder);

    protected string GetPropertyName<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyExpression)
        => (propertyExpression.Body as MemberExpression).Member.Name;
    //protected string GetToPropertyName<TProperty>(Expression<Func<TTo, TProperty>> propertyExpression)
    //    => (propertyExpression.Body as MemberExpression).Member.Name;
    protected string GetNamespace(Type type)
        => type.Namespace == null ? null : 
            string.Join('.', SyntaxFactory
            .ParseName(provider.GetTypeOutput(new CodeTypeReference(type)))
            .NormalizeWhitespace().ToFullString().Split('.')[..^1]);

    protected string GetTypeSyntax(Type type)
        => SyntaxFactory
            .ParseName(provider.GetTypeOutput(new CodeTypeReference(type)))
            .NormalizeWhitespace().ToFullString();

    protected string GetTypeSyntaxWithoutSpecialCharacter(Type type)
    {
       var typeName =  SyntaxFactory
            .ParseName(provider.GetTypeOutput(new CodeTypeReference(type)))
            .NormalizeWhitespace().ToFullString().Split('.').Last();

       Regex r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

       return r.Replace(typeName, String.Empty).Trim();
    }

    public void Dispose() =>  provider.Dispose();
}
