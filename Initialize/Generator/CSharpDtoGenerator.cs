using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Initialize.Generators;
public interface ICSharpGenerator
{
    IEnumerable<MetadataReference> References { get; set; }
    CompilationUnitSyntax Generate();
}
public class CSharpDtoGenerator : ICSharpGenerator
{
    private Type _srcType;
    private string _nameSuffix;
    private string _namesSpace;
    private bool _notifyOnPropertyChanged;
    public CSharpDtoGenerator(Type srcType, string nameSuffix, string namesSpace, bool notifyOnPropertyChanged = true)
    {
        _srcType = srcType;
        _nameSuffix = nameSuffix;
        _namesSpace = namesSpace;
        _notifyOnPropertyChanged = notifyOnPropertyChanged;
    }

    public IEnumerable<MetadataReference> References { get; set; }

    public CompilationUnitSyntax Generate()
    {
        References = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Private_CoreLib),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Runtime),
            MetadataReference.CreateFromFile(typeof(DataContractAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(MulticastDelegate).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.ComponentModel.INotifyPropertyChanged).Assembly.Location),
            MetadataReference.CreateFromFile(_srcType.Assembly.Location),
        };
        return CompilationUnit()
        .WithUsings
        (
            List<UsingDirectiveSyntax>
            (
                new UsingDirectiveSyntax[]
                {
                UsingDirective
                (
                    IdentifierName("System")
                ),
                UsingDirective
                (
                    QualifiedName
                    (
                        IdentifierName("System"),
                        IdentifierName("ComponentModel")
                    )
                ),
                UsingDirective
                (
                    QualifiedName
                    (
                        IdentifierName("System"),
                        IdentifierName("Runtime.CompilerServices")
                    )
                ),
                UsingDirective
                (
                    QualifiedName
                    (
                        IdentifierName("System"),
                        IdentifierName("Runtime.Serialization")
                    )
                )
                }
            )
        )
        .WithMembers
        (
            SingletonList<MemberDeclarationSyntax>
            (
                FileScopedNamespaceDeclaration
                (
                    IdentifierName(_namesSpace)
                )
                .WithMembers
                (
                    SingletonList<MemberDeclarationSyntax>
                    (
                        _notifyOnPropertyChanged
                        ? ClassDeclarationWithNotifyPropertyChanged()
                        : ClassDeclarationWithoutNotifyPropertyChanged()
                    )
                )
            )
        );
    }

    private ClassDeclarationSyntax ClassDeclarationWithoutNotifyPropertyChanged()
        => ClassDeclaration($"{_srcType.Name}{_nameSuffix}")
            .WithAttributeLists
            (
                SingletonList<AttributeListSyntax>
                (
                    AttributeList
                    (
                        SingletonSeparatedList<AttributeSyntax>
                        (
                            Attribute
                            (
                                IdentifierName("DataContract")
                            )
                        )
                    )
                )
            )
            .WithModifiers
            (
                TokenList
                (
                    Token(SyntaxKind.PublicKeyword)
                )
            )
            .WithMembers
            (
                        List(Members(notifyPropertyChanged: false))
            );
    
    private ClassDeclarationSyntax ClassDeclarationWithNotifyPropertyChanged()
        => ClassDeclaration($"{_srcType.Name}{_nameSuffix}")
            .WithAttributeLists
            (
                SingletonList<AttributeListSyntax>
                (
                    AttributeList
                    (
                        SingletonSeparatedList<AttributeSyntax>
                        (
                            Attribute
                            (
                                IdentifierName("DataContract")
                            )
                        )
                    )
                )
            )
            .WithModifiers
            (
                TokenList
                (
                    Token(SyntaxKind.PublicKeyword)
                )
            )
            .WithBaseList
            (
                BaseList
                (
                    SingletonSeparatedList<BaseTypeSyntax>
                    (
                        SimpleBaseType
                        (
                            IdentifierName("INotifyPropertyChanged")
                        )
                    )
                )
            )
            .WithMembers
            (
                List(Members(notifyPropertyChanged: true))
            );

    private IEnumerable<MemberDeclarationSyntax> Members(int startIndex = 0, bool notifyPropertyChanged = true)
    => List<MemberDeclarationSyntax>
    (
        new MemberDeclarationSyntax[]{
                ConstructorDeclaration
                (
                    Identifier($"{_srcType.Name}{_nameSuffix}")
                )
                .WithModifiers
                (
                    TokenList
                    (
                        Token(SyntaxKind.PublicKeyword)
                    )
                )
                .WithBody
                (
                    Block()
                ),
                ConstructorDeclaration
                (
                    Identifier($"{_srcType.Name}{_nameSuffix}")
                )
                .WithModifiers
                (
                    TokenList
                    (
                        Token(SyntaxKind.PublicKeyword)
                    )
                )
                .WithParameterList
                (
                    ParameterList
                    (
                        SeparatedList<ParameterSyntax>
                        (
                            MethodArguments(new List<(Type, string)> { (_srcType, _srcType.Name) })
                        )
                    )
                )
                .WithBody
                (
                    Block
                    (
                        AssignmentBody(_srcType.GetProperties().Select(r => r.Name))
                    )
                )
            }
            .Union(
            _srcType.GetProperties().SelectMany(prop =>
            new MemberDeclarationSyntax[]
            {

                DeclareField(prop),
                DeclareProperty(prop, notifyPropertyChanged)
                .WithAttributeLists
                (
                    SingletonList<AttributeListSyntax>
                    (
                        AttributeList
                        (
                            SingletonSeparatedList<AttributeSyntax>
                            (
                                Attribute
                                (
                                    IdentifierName("DataMember")
                                )
                                .WithArgumentList
                                (
                                    AttributeArgumentList
                                    (
                                        SingletonSeparatedList<AttributeArgumentSyntax>
                                        (
                                            AttributeArgument
                                            (
                                                LiteralExpression
                                                (
                                                    SyntaxKind.NumericLiteralExpression,
                                                    Literal(startIndex++)
                                                )
                                            )
                                            .WithNameEquals
                                            (
                                                NameEquals
                                                (
                                                    IdentifierName("Order")
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            }
        )
    )
    .Union( notifyPropertyChanged 
                ?
                new MemberDeclarationSyntax[]
                {
                    PropertyChangedEvent(),
                    RaisePropertyChange()
                } 
                :
                new MemberDeclarationSyntax[]{}
    )
);

    private PropertyDeclarationSyntax DeclareProperty(PropertyInfo prop, bool notifyPropertyChanged = true)
        => PropertyDeclaration
        (
            IdentifierName(GetTypeName(prop.PropertyType)),
            Identifier(prop.Name)
        )
        .WithModifiers
        (
            TokenList
            (
                Token(SyntaxKind.PublicKeyword)
            )
        )
        .WithAccessorList
        (
            AccessorList
            (
                List<AccessorDeclarationSyntax>
                (
                    new AccessorDeclarationSyntax[]
                    {
                    AccessorDeclaration
                    (
                        SyntaxKind.GetAccessorDeclaration
                    )
                    .WithExpressionBody
                    (
                        ArrowExpressionClause
                        (
                            IdentifierName($"_{prop.Name[0..1].ToLower()}{prop.Name[1..]}")
                        )
                    )
                    .WithSemicolonToken
                    (
                        Token(SyntaxKind.SemicolonToken)
                    ),
                    AccessorDeclaration
                    (
                        SyntaxKind.SetAccessorDeclaration
                    )
                    .WithBody
                    (
                        notifyPropertyChanged
                            ?
                        Block
                        (
                            ExpressionStatement
                            (
                                AssignmentExpression
                                (
                                    SyntaxKind.SimpleAssignmentExpression,
                                    IdentifierName($"_{prop.Name[0..1].ToLower()}{prop.Name[1..]}"),
                                    IdentifierName("value")
                                )
                            ),
                            ExpressionStatement
                            (
                                InvocationExpression
                                (
                                    IdentifierName("RaisePropertyChanged")
                                )
                            )
                        )
                        :
                        Block
                            (
                                ExpressionStatement
                                (
                                    AssignmentExpression
                                    (
                                        SyntaxKind.SimpleAssignmentExpression,
                                        IdentifierName($"_{prop.Name[0..1].ToLower()}{prop.Name[1..]}"),
                                        IdentifierName("value")
                                    )
                                )
                            )
                        )
                    }
                )
            )
        );

    private FieldDeclarationSyntax DeclareField(PropertyInfo type)
        => FieldDeclaration
        (
            VariableDeclaration
            (
                IdentifierName(GetTypeName(type.PropertyType))
            )
            .WithVariables
            (
                SingletonSeparatedList<VariableDeclaratorSyntax>
                (
                    VariableDeclarator
                    (
                        Identifier($"_{type.Name[0..1].ToLower()}{type.Name[1..]}")
                    )
                )
            )
        )
        .WithModifiers
        (
            TokenList
            (
                Token(SyntaxKind.PrivateKeyword)
            )
        );

    private StatementSyntax[] AssignmentBody(IEnumerable<string> properties)
        => properties.Select(prop => ExpressionStatement
            (
                AssignmentExpression
                (
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName($"_{prop[0..1].ToLower()}{prop[1..]}"),
                    MemberAccessExpression
                    (
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("obj"),
                        IdentifierName(prop)
                    )
                )
            )
        )
        .ToArray();

    private SyntaxNodeOrToken[] MethodArguments(IEnumerable<(Type, string)> args)
    => args.Take(1)
                .SelectMany(arg =>
                    new SyntaxNodeOrToken[] {
                            Parameter
                            (
                                    Identifier("obj")
                                )
                                .WithType
                                (
                                    IdentifierName(GetTypeName(arg.Item1))
                                )
                        }
                    )
                    .Union(args.Skip(1)
                    .SelectMany(arg =>
                    {
                        return new SyntaxNodeOrToken[]
                        {
                                Token(SyntaxKind.CommaToken),
                                Parameter
                                (
                                    Identifier(arg.Item2)
                                )
                                .WithType
                                (
                                    IdentifierName(GetTypeName(arg.Item1))
                                )
                        };
                    }
                ).ToList()
            ).ToArray();

    private EventFieldDeclarationSyntax PropertyChangedEvent()
    => EventFieldDeclaration
    (
        VariableDeclaration
        (
            IdentifierName("PropertyChangedEventHandler")
        )
        .WithVariables
        (
            SingletonSeparatedList<VariableDeclaratorSyntax>
            (
                VariableDeclarator
                (
                    Identifier("PropertyChanged")
                )
            )
        )
    )
    .WithModifiers
    (
        TokenList
        (
            Token(SyntaxKind.PublicKeyword)
        )
    );

    private MemberDeclarationSyntax RaisePropertyChange()
        => MethodDeclaration
        (
            PredefinedType
            (
                Token(SyntaxKind.VoidKeyword)
            ),
            Identifier("RaisePropertyChanged")
        )
        .WithModifiers
        (
            TokenList
            (
                Token(SyntaxKind.PublicKeyword)
            )
        )
        .WithParameterList
        (
            ParameterList
            (
                SingletonSeparatedList<ParameterSyntax>
                (
                    Parameter
                    (
                        Identifier("propertyName")
                    )
                    .WithAttributeLists
                    (
                        SingletonList<AttributeListSyntax>
                        (
                            AttributeList
                            (
                                SingletonSeparatedList<AttributeSyntax>
                                (
                                    Attribute
                                    (
                                        IdentifierName("CallerMemberName")
                                    )
                                )
                            )
                        )
                    )
                    .WithType
                    (
                        PredefinedType
                        (
                            Token(SyntaxKind.StringKeyword)
                        )
                    )
                    .WithDefault
                    (
                        EqualsValueClause
                        (
                            LiteralExpression
                            (
                                SyntaxKind.NullLiteralExpression
                            )
                        )
                    )
                )
            )
        )
        .WithBody
        (
            Block
            (
                SingletonList<StatementSyntax>
                (
                    IfStatement
                    (
                        BinaryExpression
                        (
                            SyntaxKind.NotEqualsExpression,
                            IdentifierName("PropertyChanged"),
                            LiteralExpression
                            (
                                SyntaxKind.NullLiteralExpression
                            )
                        ),
                        Block
                        (
                            SingletonList<StatementSyntax>
                            (
                                ExpressionStatement
                                (
                                    InvocationExpression
                                    (
                                        IdentifierName("PropertyChanged")
                                    )
                                    .WithArgumentList
                                    (
                                        ArgumentList
                                        (
                                            SeparatedList<ArgumentSyntax>
                                            (
                                                new SyntaxNodeOrToken[]
                                                {
                                                Argument
                                                (
                                                    ThisExpression()
                                                ),
                                                Token(SyntaxKind.CommaToken),
                                                Argument
                                                (
                                                    ObjectCreationExpression
                                                    (
                                                        IdentifierName("PropertyChangedEventArgs")
                                                    )
                                                    .WithArgumentList
                                                    (
                                                        ArgumentList
                                                        (
                                                            SingletonSeparatedList<ArgumentSyntax>
                                                            (
                                                                Argument
                                                                (
                                                                    IdentifierName("propertyName")
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                                }
                                            )
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            )
        );

    private string GetTypeName(Type type)
    {
        using CSharpCodeProvider provider = new Microsoft.CSharp.CSharpCodeProvider();
        return provider.GetTypeOutput(new System.CodeDom.CodeTypeReference(type));
    }
}
