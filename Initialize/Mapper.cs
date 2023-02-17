using System.Buffers;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
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

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IEnumerable<TTo> MapEnumerable(List<TFrom> objFrom)
    //{
    //    var span = objFrom.ToArray();
    //    TTo[] spanTo = ArrayPool<TTo>.Shared.Rent(objFrom.Count);
    //    var cnt = objFrom.Count;
    //    var current = 0;

    //    foreach (var item in CollectionsMarshal.AsSpan(objFrom))
    //        spanTo[current++] = Map(item);
    //    //for (int i = 0; i < cnt; i++)
    //    //    spanTo[i] =  Map(span[i]);
        
    //    ArrayPool<TTo>.Shared.Return(spanTo);

    //    return spanTo;
    //}
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IEnumerable<TTo> MapEnumerable(IList<TFrom> objFrom)
    //{
    //    var span = objFrom.ToArray();
    //    TTo[] spanTo = new TTo[span.Length];
    //    var cnt = span.Length;

    //    for (int i = 0; i < cnt; i++)
    //        spanTo[i] =  Map(span[i]);
        
    //    //ArrayPool<TTo>.Shared.Return(spanTo);

    //    return spanTo;
    //}
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IEnumerable<TTo> MapEnumerable(TFrom[] objFrom)
    //{
    //    var span = objFrom;
    //    TTo[] spanTo = new TTo[span.Length];
    //    var cnt = span.Length;

    //    for (int i = 0; i < cnt; i++)
    //        spanTo[i] =  Map(span[i]);
        
    //    //ArrayPool<TTo>.Shared.Return(spanTo);

    //    return spanTo;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TTo> MapEnumerable(IEnumerable<TFrom> objFrom)
    {
        //if (objFrom is TFrom[] array)
        //{
        //    return MapEnumerable(array);
        //}
        
        //if (objFrom is IList<TFrom> iList)
        //{
        //    if (iList is List<TFrom> list)
        //    {
        //        return MapEnumerable(list);
        //    }
        //    return MapEnumerable(iList);
        //}

        var span = objFrom.ToArray().AsSpan();
        var spanTo = ArrayPool<TTo>.Shared.Rent(span.Length);
        var cnt = span.Length;

        for (int i = 0; i < cnt; i++)
            spanTo[i] =  Map(span[i]);
        
        ArrayPool<TTo>.Shared.Return(spanTo);

        return spanTo;
    }
 
    public static IEnumerable<TTo> MapArray(IEnumerable<TFrom> objFrom)
    {
        int cnt = 0, current = 0;
        var span = objFrom.ToArray().AsSpan();
        Span<TTo> spanTo = new TTo[span.Length];// ArrayPool<TTo>.Shared.Rent(span.Length);

        for (int i = 0; i < cnt; i++)
        {
            spanTo[i] = Map(span[i]);
        }

        //ArrayPool<TTo>.Shared.Return(spanTo);
        
        return spanTo.ToArray();// Map(objFrom.ToList());
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TTo> MapArrayInline(IEnumerable<TFrom> objFrom)
    {
        int cnt = 0, current = 0;
        var span = objFrom.ToArray();
        TTo[] spanTo = ArrayPool<TTo>.Shared.Rent(span.Length);
        for (int i = 0; i < cnt; i++)
            spanTo[i] =  Map(span[i]);
        
        ArrayPool<TTo>.Shared.Return(spanTo);

        return spanTo;
    }
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static IEnumerable<TTo> MapArrayOpt(IEnumerable<TFrom> objFrom)
    {
        int cnt = 0, current = 0;
        var span = objFrom.ToArray().AsSpan();
        TTo[] spanTo = ArrayPool<TTo>.Shared.Rent(span.Length);
        for (int i = 0; i < cnt; i++)
            spanTo[i] =  Map(span[i]);
        
        ArrayPool<TTo>.Shared.Return(spanTo);

        return spanTo;// Map(objFrom.ToList());
    }
    ///// <summary>
    ///// Initiaze properties on <typeparam name="T"></typeparam> to non-null values.
    ///// </summary>
    ///// <param name="obj">object or value type to be serialized</param>
    ///// <returns><seealso cref="Span{byte}"/></returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static IEnumerable<TTo> Map2(IEnumerable<TFrom> objFrom)
    //{
    //    var pool = ArrayPool<TTo>.Shared;
    //    int cnt = 0, size = 100;
    //    //var from = objFrom.ToArray();
    //    var list = new List<TTo>();
    //    //var array = pool.Rent(size);
    //    //MemoryMarshal.Cast<long, Vector<long>>(Values);
    //    var from = CollectionsMarshal.AsSpan(objFrom.ToList());

    //    foreach (var item in from)
    //    {
    //        list.Add(Map(item));
    //    }

    //    //for (int i = 0; i < from.Length; i++)
    //    //{
    //    //    if (cnt == size)
    //    //    {
    //    //        cnt = 0;
    //    //        list.AddRange(array[..size]);
    //    //        pool.Return(array);
    //    //        array = pool.Rent(size);
    //    //    }
    //    //    var objTo = new TTo();
    //    //    MapFromTo(from[i], objTo);

    //    //    array[cnt] = objTo;
    //    //    cnt++;
    //    //}

    //    //if (cnt > 0)
    //    //{
    //    //    list.AddRange(array[..cnt]);
    //    //    pool.Return(array);
    //    //}

    //    return list;
    //}
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BuildDelegates()
    {
        var methodInfo = _proxyType.GetMethod(
            name: nameof(Map), 
            bindingAttr: BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public, 
            types: new []{ typeof(TFrom), typeof(TTo)});
        
        _mapFromTo = methodInfo.CreateDelegate<Action<TFrom, TTo>>();

        methodInfo = _proxyType.GetMethod(
            name: nameof(Map), 
            bindingAttr: BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public, 
            types: new []{ typeof(TFrom)});
        
        _mapFrom = methodInfo.CreateDelegate<Func<TFrom, TTo>>();
    }

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
        
        references.AddRange(typeof(TFrom).GetProperties().Select(r=>MetadataReference.CreateFromFile(r.PropertyType.Assembly.Location)));

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

            _context = new AssemblyLoadContext("Mapper", false);

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
        public static string System_Collections => Path.Combine(Path.GetDirectoryName(typeof(System.Runtime.GCSettings).GetTypeInfo().Assembly.Location), "System.Collections.dll");
    }
}