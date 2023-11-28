using System.Linq.Expressions;
using System.Text;
using Initialize.DelimitedParser;

namespace Initialize.Mapper;

public class MapperManualTemplate<TFrom, TTo> : MapperTemplateBase<TFrom, TTo>
{
    private static readonly string SyntaxPropertyBind = $"{SyntaxVarTo}.{{0}} = {SyntaxVarFrom}.{{1}};";
    private static readonly string SyntaxPropertyAssign = $"{SyntaxVarTo}.{{0}} = {{1}};";
    private readonly StringBuilder _syntaxBuilder = new ();

    public MapperManualTemplate<TFrom, TTo> For<TProperty>(Expression<Func<TTo, TProperty>> propTo, Expression<Func<TFrom, TProperty>> propFrom)
    {
        var fromName = this.GetPropertyName(propFrom);
        var toName = this.GetPropertyName(propTo);

		this._syntaxBuilder.AppendFormat(SyntaxPropertyBind , toName, fromName);

        return this;
    }

    private int index = 0;
    public MapperManualTemplate<TFrom, TTo> For<TProperty>(Expression<Func<TTo, TProperty>> propTo, string rightSideOfAssignment)
    {
        var toName = this.GetPropertyName(propTo);

		this._syntaxBuilder.AppendFormat(SyntaxPropertyAssign, toName, rightSideOfAssignment);

        return this;
    }
    public MapperManualTemplate<TFrom, TTo> For<TProperty>(Expression<Func<TTo, TProperty>> propTo, string rightSideAssignment, int indexOffset)
    {
        if(typeof(TFrom).GetProperties().Any(p => p.GetIndexParameters().Length != 0)) 
            throw new ArgumentException("From type must be a type with an indexer");

        var toName = this.GetPropertyName(propTo);
        
        var rightStr = string.Format(rightSideAssignment, SyntaxVarFrom, indexOffset + this.index++);

		this._syntaxBuilder.AppendFormat(SyntaxPropertyAssign, toName, rightStr);

        return this;
    }
    public MapperManualTemplate<TFrom, TTo> ParseFor<TProperty>(Expression<Func<TTo, TProperty>> propTo, Func<ParseUTF8, Func<int, string>> rightSideOfAssignment,  int indexOffset = 0)
    {
        if(typeof(TFrom).GetProperties().Any(p => p.GetIndexParameters().Length != 0)) 
            throw new ArgumentException("From type must be a type with an indexer");

        var toName = this.GetPropertyName(propTo);

        var parser = new ParseUTF8();

        var right = rightSideOfAssignment(parser);
        
        var rightStr = string.Format(right(indexOffset + this.index++), SyntaxVarFrom);

		this._syntaxBuilder.Append(string.Format(SyntaxPropertyAssign, toName, rightStr).AsSpan());

        return this;
    }
    public MapperManualTemplate<TFrom, TTo> ParseFor<TProperty>(Expression<Func<TTo, TProperty>> propTo, Func<ParseUTF8, Func<int, string, string>> rightSideOfAssignment, string parseFormat, int indexOffset = 0)
    {
        if(typeof(TFrom).GetProperties().Any(p => p.GetIndexParameters().Length != 0)) 
            throw new ArgumentException("From type must be a type with an indexer");

        var toName = this.GetPropertyName(propTo);

        var parser = new ParseUTF8();

        var right = rightSideOfAssignment(parser);
        
        var rightStr = string.Format(right(indexOffset + this.index++, "\"" + parseFormat + "\""), SyntaxVarFrom);

		this._syntaxBuilder.Append(string.Format(SyntaxPropertyAssign, toName, rightStr).AsSpan());

        return this;
    }
    protected override void GenerateBody(out StringBuilder syntaxBuilder) 
        => syntaxBuilder = this._syntaxBuilder;
}