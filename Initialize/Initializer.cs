﻿using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Initialize;
public static class Initializer<T>
{
    private static string _proxyTypeName;
    private static Assembly _generatedAssembly;
    private static AssemblyLoadContext _context;
    private static Type _proxyType;
    internal delegate void Del(T obj);
    internal static Del CacheDel;

    static Initializer()
    => Compile();

    public static InitializerTemplate<T> Template { get; set; } = new();

    /// <summary>
    /// Initiaze properties on <typeparam name="T"></typeparam> to non-null values.
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Initialize(T obj)
        => CacheDel(obj);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BuildDelegates()
    {
        var infos = _proxyType.GetMethod(nameof(Initialize));
        CacheDel = infos.CreateDelegate<Del>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Compile()
    {
        _proxyTypeName = $"Initializer{typeof(T).Name}.Init{typeof(T).Name}";

        var result = Template.Generate();

        var references = new List<PortableExecutableReference> {
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Private_CoreLib),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Runtime),
            MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Initializer<>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(T).GetTypeInfo().Assembly.Location)};

        var csharpSyntax = CSharpSyntaxTree.ParseText(result);

#if DEBUG
		Debug.Write(csharpSyntax.GetRoot().NormalizeWhitespace().ToFullString());
#endif

        var compilation = CSharpCompilation.Create(
                $"{_proxyTypeName}_{DateTime.Now.ToFileTimeUtc()}",
            new[] { CSharpSyntaxTree.ParseText(result) },
            references,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                allowUnsafe: true,
                optimizationLevel: OptimizationLevel.Release)
        );
        Emit(compilation);
        BuildDelegates();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Emit(CSharpCompilation _compilation)
    {
        if (CacheDel != null) return;
        using (var ms = new MemoryStream())
        {
            var result = _compilation.Emit(ms);
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
                    var exception = new Exception($"Compilation failed, first error is: {firstErrorMessage}");
                    compilationErrors.ForEach(e => { if (!exception.Data.Contains(e.Id)) exception.Data.Add(e.Id, e.GetMessage()); });
                    throw exception;
                }
            }

            _context = new AssemblyLoadContext("Initializer", true);

            ms.Seek(0, SeekOrigin.Begin);

            _generatedAssembly = _context.LoadFromStream(ms);

            _proxyType = _generatedAssembly.GetType(_proxyTypeName);
        }
    }

    internal class FrameworkAssemblyPaths
    {
        public static string System => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.dll");
        public static string System_Runtime => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll");
        public static string System_Private_CoreLib => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.CoreLib.dll");
    }
}