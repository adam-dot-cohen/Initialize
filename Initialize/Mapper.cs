using System.Buffers;
using System.Collections;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Initialize.Mapper;
using Initialize.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Initialize;
/// <summary>
/// Maps from <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>.
/// </summary>
/// <typeparam name="TFrom"></typeparam>
/// <typeparam name="TTo"></typeparam>
public static class Mapper<TFrom, TTo>
{
    private static string _proxyTypeName;
    private static Assembly _generatedAssembly;
    private static AssemblyLoadContext _context;
    private static Type _proxyType;
    private static Action<TFrom, TTo> _mapFromTo;
    private static Func<TFrom, TTo> _mapFrom;
    private static Func<List<TFrom>, TFrom[]> _privateIndexAccessor;
    private static Func<List<TTo>, TTo[]> _privateIndexAccessorTo;
    private static Func<List<TTo>, int> _privateIndexAccessorToSize;

    private delegate TTo MapFromByRefDel(ref TFrom objFrom);

    private static MapFromByRefDel _mapFromByRef;
   
    static Mapper() => Compile();

    /// <summary>
    /// Maps caller supplied <typeparamref name="TFrom"/> to caller supplied <typeparamref name="TTo"/> object.
    /// </summary>
    /// <param name="objFrom">Source <typeparamref name="TFrom"/> object</param>
    /// <param name="objTo">Destination <typeparamref name="TTo"/> object</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Map(TFrom objFrom, TTo objTo) => _mapFromTo(objFrom, objTo);

    /// <summary>
    /// Maps caller supplied <typeparamref name="TFrom"/> to returned <typeparamref name="TTo"/> object.
    /// </summary>
    /// <param name="objFrom">Source <typeparamref name="TFrom"/> object</param>
    /// <returns name="objTo">A <typeparamref name="TTo"/> object with hydrated properties.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTo Map(TFrom objFrom) => _mapFrom(objFrom);

    /// <summary>
    /// Maps caller supplied <typeparamref name="TFrom"/> to returned <typeparamref name="TTo"/> object.
    /// </summary>
    /// <param name="objFrom">By-ref source <typeparamref name="TFrom"/> object</param>
    /// <returns name="objTo">A <typeparamref name="TTo"/> object with hydrated properties.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTo Map(ref TFrom objFrom) => _mapFromByRef(ref objFrom);

    /// <summary>
    /// Maps caller supplied enumerable <typeparamref name="TFrom"/> to enumerable of <typeparamref name="TTo"/>
    /// </summary>
    /// <param name="objFrom">Enumerable of objects to be dynamically mapped</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TTo> Map(List<TFrom> objFrom)
    {
        //var list = new TTo[objFrom.Count];
        //if(objFrom.Count <= 100) 
        //    return objFrom.ConvertAll(Map);
        //var list = new List<TTo>(objFrom.Count);
        //var index = _privateIndexAccessor(objFrom);
        //foreach(var item in Map(index)) list.Add(item);
        //return list;
        //return list;
        //for (int x = 0; x < objFrom.Count; x++) list[x] = Map(objFrom[x]);
        //return list;
        //Unsafe.As<>()
        //return MemoryMarshal.c<TTo>(list.Span);
        for (int x = 0; x < objFrom.Count; x++) yield return Map(objFrom[x]);
        //foreach(var item in objFrom) yield return Map(item);
    }

    /// <summary>
    /// Maps caller supplied enumerable <typeparamref name="TFrom"/> to enumerable of <typeparamref name="TTo"/>
    /// </summary>
    /// <param name="objFrom">Enumerable of objects to be dynamically mapped</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TTo> Map(TFrom[] objFrom)
    {
        for (int x = 0; x < objFrom.Length; x++) yield return Map(objFrom[x]);
    }

    /// <summary>
    /// Maps caller supplied enumerable <typeparamref name="TFrom"/> to enumerable of <typeparamref name="TTo"/>
    /// </summary>
    /// <param name="objFrom">Enumerable of objects to be dynamically mapped</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TTo> Map(IEnumerable<TFrom> objFrom)
    {
        foreach (var item in objFrom) yield return Map(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BuildDelegates()
    {
        var methodInfo = _proxyType.GetMethod(
            name: nameof(Map),
            bindingAttr: BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public,
            types: new[] { typeof(TFrom), typeof(TTo) });

        _mapFromTo = methodInfo.CreateDelegate<Action<TFrom, TTo>>();


        methodInfo = _proxyType.GetMethod(
            name: nameof(Map),
            bindingAttr: BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public,
            types: new[] { typeof(TFrom) });

        _mapFrom = methodInfo.CreateDelegate<Func<TFrom, TTo>>();


        methodInfo = _proxyType.GetMethod(
            name: nameof(Map),
            bindingAttr: BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public,
            types: new[] { typeof(TFrom).MakeByRefType() });

        _mapFromByRef = methodInfo.CreateDelegate<MapFromByRefDel>();
     
        
        InitIndeAccessor("_items");
    }
    public static void InitIndeAccessor(string name) 
    { 
        ParameterExpression param = 
            Expression.Parameter (typeof(List<TFrom>),"arg");  

        MemberExpression member = 
            Expression.Field(param, name);   

        LambdaExpression lambda = 
            Expression.Lambda(typeof(Func<List<TFrom>, TFrom[]>), member, param);   

        _privateIndexAccessor = (Func<List<TFrom>, TFrom[]>)lambda.Compile(); 
    }
    //public static void MemberAssign<TItem, TToType>(Func<List<TItem>, TToType> indexAccessor, string name) 
    //{ 
    //    ParameterExpression param = 
    //        Expression.Parameter (typeof(List<TItem>),"arg");  

    //    MemberExpression member = 
    //        Expression.Field(param, name);
        
    //    LambdaExpression lambda = 
    //        Expression.Lambda(typeof(Action<List<TItem>,TToType>), member, param);   

    //    indexAccessor = (Func<List<TItem>, TToType>)lambda.Compile(); 
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Compile()
    {
        var result = MapperConfiguration<TFrom, TTo>.Template.Generate();

        _proxyTypeName = MapperConfiguration<TFrom, TTo>.Template.FullName;

        var references = new List<PortableExecutableReference> {
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Private_CoreLib),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Runtime),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Collections),
            MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Text.Encoding).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Dictionary<,>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Initializer<>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(TFrom).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(TTo).GetTypeInfo().Assembly.Location) };

        references.AddRange(typeof(TFrom).GetProperties().Select(r => MetadataReference.CreateFromFile(r.PropertyType.Assembly.Location)));

        references = references.Distinct().ToList();

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
        if (_mapFromTo != null) return;
        using var ms = new MemoryStream();
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

        _context = new AssemblyLoadContext("Mapper", false);

        ms.Seek(0, SeekOrigin.Begin);

        _generatedAssembly = _context.LoadFromStream(ms);

        _proxyType = _generatedAssembly.GetType(_proxyTypeName);
    }

    internal class FrameworkAssemblyPaths
    {
        public static string System => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.dll");
        public static string System_Runtime => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Runtime.dll");
        public static string System_Private_CoreLib => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Private.CoreLib.dll");
        public static string System_Collections => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Collections.dll");
    }
}