using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Initialize;

public class MapperManualTemplate<TFrom, TTo> : MapperTemplateBase<TFrom, TTo>
{
    const string SyntaxPropertyBind = "objTo.{0} = objFrom.{1};";
    const string SyntaxPropertyAssign = "objTo.{0} = {1};";
    const string SyntaxFrom = "objFrom";

    StringBuilder _maps = new StringBuilder();

    public MapperManualTemplate<TFrom, TTo> For<TProperty>(Expression<Func<TTo, TProperty>> propTo, Expression<Func<TFrom, TProperty>> propFrom)
    {
        var fromName = GetPropertyName(propFrom);
        var toName = GetPropertyName(propTo);

        _maps.AppendFormat(SyntaxPropertyBind , toName, fromName);

        return this;
    }

    private static int index = 0;
    public MapperManualTemplate<TFrom, TTo> AddMap<TProperty>(Expression<Func<TTo, TProperty>> propTo, string rightSideOfAssignment)
    {
        var toName = GetPropertyName(propTo);

        _maps.AppendFormat(SyntaxPropertyAssign, toName, rightSideOfAssignment);

        return this;
    }
    public MapperManualTemplate<TFrom, TTo> For<TProperty>(Expression<Func<TTo, TProperty>> propTo, string rightSideAssignment, int indexOffset)
    {
        if(typeof(TFrom).GetProperties().Any(p => p.GetIndexParameters().Length != 0)) 
            throw new ArgumentException("From type must be a type with an indexer");

        var toName = GetPropertyName(propTo);
        
        var rightStr = string.Format(rightSideAssignment, SyntaxFrom, indexOffset + index++);

        _maps.AppendFormat(SyntaxPropertyAssign, toName, rightStr);

        return this;
    }
    public MapperManualTemplate<TFrom, TTo> ParseFor<TProperty>(Expression<Func<TTo, TProperty>> propTo, Func<ParseUTF8, Func<int, string>> rightSideOfAssignment,  int indexOffset = 0)
    {
        if(typeof(TFrom).GetProperties().Any(p => p.GetIndexParameters().Length != 0)) 
            throw new ArgumentException("From type must be a type with an indexer");

        var toName = GetPropertyName(propTo);

        var parser = new ParseUTF8();

        var right = rightSideOfAssignment(parser);
        
        var rightStr = string.Format(right(indexOffset + index++), SyntaxFrom);

        _maps.AppendFormat(SyntaxPropertyAssign, toName, rightStr);

        return this;
    }
    public MapperManualTemplate<TFrom, TTo> ParseFor<TProperty>(Expression<Func<TTo, TProperty>> propTo, Func<ParseUTF8, Func<int, string, string>> rightSideOfAssignment, string parseFormat, int indexOffset = 0)
    {
        if(typeof(TFrom).GetProperties().Any(p => p.GetIndexParameters().Length != 0)) 
            throw new ArgumentException("From type must be a type with an indexer");

        var toName = GetPropertyName(propTo);

        var parser = new ParseUTF8();

        var right = rightSideOfAssignment(parser);
        
        var rightStr = string.Format(right(indexOffset + index++, parseFormat), SyntaxFrom);

        _maps.AppendFormat(SyntaxPropertyAssign, toName, rightStr);

        return this;
    }
    protected override void GenerateBody(StringBuilder syntaxBuilder) 
        => syntaxBuilder.Append(_maps.ToString());
}