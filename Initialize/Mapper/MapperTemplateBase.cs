using System.CodeDom;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CSharp;

namespace Initialize.Mapper;

public interface IMapperTemplate<TFrom, TTo>
{
    string FullName { get; }
    string Generate();
}

public abstract class MapperTemplateBase<TFrom, TTo> : IMapperTemplate<TFrom, TTo>
{
    CSharpCodeProvider _provider = new ();
	Regex _regex = new ("(?:[^a-z0-9 ]|(?<=['\"])s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private string _namespace;
    private string _className;
    protected  const string SyntaxVarFrom = "objFrom";
    protected const string SyntaxVarTo = "objTo";

    protected MapperTemplateBase()
    {
		this._namespace =  $"Mapper.{this.GetTypeSyntaxWithoutSpecialCharacter(typeof(TFrom)).Trim()}{this.GetTypeSyntaxWithoutSpecialCharacter(typeof(TTo)).Trim()}";
		this._className = $"{this.GetTypeSyntaxWithoutSpecialCharacter(typeof(TFrom))}_{this.GetTypeSyntaxWithoutSpecialCharacter(typeof(TTo))}";
    }

    public string FullName => $"{this._namespace}.{this._className}";
       

    public string Generate()
    {
        var type = typeof(TFrom);
        var typeTo = typeof(TTo);
        var syntaxTypeFrom = this.GetTypeSyntax(type);
        var syntaxTypeTo = this.GetTypeSyntax(typeTo);
        var namespaceFrom = this.GetNamespace(type);
        var namespaceTo = this.GetNamespace(typeTo);
        var usingFrom = string.IsNullOrWhiteSpace(namespaceFrom) ? string.Empty : $"using {namespaceFrom};";
        var usingTo = string.IsNullOrWhiteSpace(namespaceTo) ? string.Empty :$"using {namespaceTo};";
        var syntaxUsing = namespaceFrom == namespaceTo ? usingFrom : usingFrom + usingTo;
        var sb = new StringBuilder();

		this.GenerateBody(out var bodySyntax);

        sb.Append($"using System;using System.Collections;using System.Collections.Generic;using System.Runtime.CompilerServices;using System.Linq;using System.Text;using Initialize;using Initialize.DelimitedParser;using Initialize.Mapper;{syntaxUsing}");
        
        sb.Append($"namespace {this._namespace}{{");
        
            sb.Append($"public static class {this._className}{{");
        
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

                    #region public static TTo Map(objFrom)
                    sb.Append("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
                    
                    sb.Append($"public static {syntaxTypeTo} Map(ref {syntaxTypeFrom} {SyntaxVarFrom}){{");
                    
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

    protected string? GetPropertyName<TSource, TProperty>(Expression<Func<TSource, TProperty>> propertyExpression)
        => (propertyExpression.Body as MemberExpression).Member.Name;

    protected string? GetNamespace(Type type)
        => type?.Namespace == null ? null : 
            string.Join('.', SyntaxFactory
			.ParseName(this._provider.GetTypeOutput(new CodeTypeReference(type)))
			.NormalizeWhitespace().ToFullString().Split('.')[..^1]);

    protected string GetTypeSyntax(Type type)
        => SyntaxFactory
            .ParseName(this._provider.GetTypeOutput(new CodeTypeReference(type)))
            .NormalizeWhitespace().ToFullString();

    protected string GetTypeSyntaxWithoutSpecialCharacter(Type type)
    {
       var typeName =  SyntaxFactory
            .ParseName(this._provider.GetTypeOutput(new CodeTypeReference(type)))
            .NormalizeWhitespace().ToFullString().Split('.').Last();

       return this._regex.Replace(typeName, String.Empty).Trim();
    }

    public void Dispose() =>  this._provider.Dispose();
}
