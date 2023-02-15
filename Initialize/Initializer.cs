using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Initialize;
public static class Initializer<T>
{
    private static string _proxyTypeName;
    private static Type _proxyType;
    private delegate void InitializeDelegate(T obj);
    private static InitializeDelegate? CachedDelegate;

    static Initializer() => Compile();

    public static InitializerTemplate<T> Template { get; set; } = new();

    /// <summary>
    /// Initiaze properties on <typeparam name="T"></typeparam> to non-null values.
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Initialize(T obj)
        => CachedDelegate(obj);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BuildDelegates()
    {
        var infos = _proxyType.GetMethod(nameof(Initialize));

        CachedDelegate = infos!.CreateDelegate<InitializeDelegate>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Compile()
    {
        _proxyTypeName =$"Initializer." + $"Initializer_{typeof(T).Name}";

        var result = Template.Generate();

        var references = new List<PortableExecutableReference> {
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Console),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Private_CoreLib),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Runtime),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(T).GetTypeInfo().Assembly.Location)
        };

        var csharpSyntax = CSharpSyntaxTree.ParseText(result);

#if DEBUG
		Debug.Write(csharpSyntax.GetRoot().NormalizeWhitespace().ToFullString());
#endif

        var compilation = CSharpCompilation.Create(
                assemblyName: $"{_proxyTypeName}_{DateTime.Now.ToFileTimeUtc()}", 
                syntaxTrees: new[] { csharpSyntax })
            .WithReferences(references)
            .WithOptions(new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true, optimizationLevel: OptimizationLevel.Release));

        Emit(compilation, csharpSyntax);
        
        BuildDelegates();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Emit(CSharpCompilation _compilation, SyntaxTree syntax)
    {
        if (CachedDelegate != null) return;
        using (var ms = new MemoryStream())
        {
            var result = _compilation.Emit(ms);

            InitializeCompilationException.ThrowIfEmitResultNullOrUnsuccessful(result, syntax);

            ms.Seek(0, SeekOrigin.Begin);

#if NET5_0_OR_GREATER
            var generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);
#else
                var generatedAssembly = Assembly.Load(ms.ToArray());
                
#endif
            _proxyType = generatedAssembly.GetType(_proxyTypeName);

            Span<byte> buffer = new byte[ms.Length];
            ms.Seek(0, SeekOrigin.Begin);
            ms.Read(buffer);
            AppDomain.CurrentDomain.Load(buffer.ToArray());
        }
    }

    
}
internal class FrameworkAssemblyPaths
{
    public static string System_Console => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Console.dll");
    public static string System => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.dll");
    public static string System_Private_CoreLib => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.CoreLib.dll");
    public static string System_Runtime_CompilerServices_Unsafe => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.CompilerServices.Unsafe.dll");
    public static string System_Runtime => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll");
}
public class InitializeCompilationException : Exception
{
    public InitializeCompilationException(string message,  SyntaxTree syntaxString) : base(message)
    {
        SyntaxTree = syntaxString;
        SyntaxTreeText = syntaxString.GetRoot().NormalizeWhitespace().ToFullString();
    }
    public SyntaxTree SyntaxTree { get; }
    public string SyntaxTreeText { get; set; }

    public static void ThrowIfEmitResultNullOrUnsuccessful(EmitResult result, SyntaxTree syntaxTree)
    {
        if (!result.Success)
        {
            var compilationErrors = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error)
                .ToList();
            if (compilationErrors.Any())
            {
                var firstError = compilationErrors.First();
                var errorNumber = firstError.Id;
                var errorDescription = firstError.GetMessage();
                var firstErrorMessage = $"{errorNumber}: {errorDescription};";
                var exception = new InitializeCompilationException($"Compilation failed, first error is: { firstErrorMessage }", syntaxTree);
                compilationErrors.ForEach(e => { if (!exception.Data.Contains(e.Id)) exception.Data.Add(e.Id, e.GetMessage()); });
                throw exception;
            }
        }
    }
}