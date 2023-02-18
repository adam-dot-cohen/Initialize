using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Initialize;

public class MapperManualTemplate<TFrom, TTo> : MapperTemplateBase<TFrom, TTo>
{
    private static readonly string SyntaxPropertyBind = $"{SyntaxVarTo}.{{0}} = {SyntaxVarFrom}.{{1}};";
    private static readonly string SyntaxPropertyAssign = $"{SyntaxVarTo}.{{0}} = {{1}};";
    private readonly StringBuilder _syntaxBuilder = new ();

    public MapperManualTemplate<TFrom, TTo> For<TProperty>(Expression<Func<TTo, TProperty>> propTo, Expression<Func<TFrom, TProperty>> propFrom)
    {
        var fromName = GetPropertyName(propFrom);
        var toName = GetPropertyName(propTo);

        _syntaxBuilder.AppendFormat(SyntaxPropertyBind , toName, fromName);

        return this;
    }

    private static int index = 0;
    public MapperManualTemplate<TFrom, TTo> For<TProperty>(Expression<Func<TTo, TProperty>> propTo, string rightSideOfAssignment)
    {
        var toName = GetPropertyName(propTo);

        _syntaxBuilder.AppendFormat(SyntaxPropertyAssign, toName, rightSideOfAssignment);

        return this;
    }
    public MapperManualTemplate<TFrom, TTo> For<TProperty>(Expression<Func<TTo, TProperty>> propTo, string rightSideAssignment, int indexOffset)
    {
        if(typeof(TFrom).GetProperties().Any(p => p.GetIndexParameters().Length != 0)) 
            throw new ArgumentException("From type must be a type with an indexer");

        var toName = GetPropertyName(propTo);
        
        var rightStr = string.Format(rightSideAssignment, SyntaxVarFrom, indexOffset + index++);

        _syntaxBuilder.AppendFormat(SyntaxPropertyAssign, toName, rightStr);

        return this;
    }
    public MapperManualTemplate<TFrom, TTo> ParseFor<TProperty>(Expression<Func<TTo, TProperty>> propTo, Func<ParseUTF8, Func<int, string>> rightSideOfAssignment,  int indexOffset = 0)
    {
        if(typeof(TFrom).GetProperties().Any(p => p.GetIndexParameters().Length != 0)) 
            throw new ArgumentException("From type must be a type with an indexer");

        var toName = GetPropertyName(propTo);

        var parser = new ParseUTF8();

        var right = rightSideOfAssignment(parser);
        
        var rightStr = string.Format(right(indexOffset + index++), SyntaxVarFrom);

        _syntaxBuilder.Append(string.Format(SyntaxPropertyAssign, toName, rightStr).AsSpan());

        return this;
    }
    public MapperManualTemplate<TFrom, TTo> ParseFor<TProperty>(Expression<Func<TTo, TProperty>> propTo, Func<ParseUTF8, Func<int, string, string>> rightSideOfAssignment, string parseFormat, int indexOffset = 0)
    {
        if(typeof(TFrom).GetProperties().Any(p => p.GetIndexParameters().Length != 0)) 
            throw new ArgumentException("From type must be a type with an indexer");

        var toName = GetPropertyName(propTo);

        var parser = new ParseUTF8();

        var right = rightSideOfAssignment(parser);
        
        var rightStr = string.Format(right(indexOffset + index++, "\"" + parseFormat + "\""), SyntaxVarFrom);

        _syntaxBuilder.Append(string.Format(SyntaxPropertyAssign, toName, rightStr).AsSpan());

        return this;
    }
    protected override void GenerateBody(out StringBuilder syntaxBuilder) 
        => syntaxBuilder =_syntaxBuilder;
}