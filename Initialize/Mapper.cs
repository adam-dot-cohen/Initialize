using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Initialize;

public static class Mapper<TFrom, TTo>
{
    private static string _proxyTypeName;
    private static Assembly _generatedAssembly;
    private static AssemblyLoadContext _context;
    private static Type _proxyType;
    private delegate void MapperDelegate(TFrom from, TTo to);
    private static MapperDelegate CacheDel;

    static Mapper()
        => Compile();

    public static MapperTemplate<TFrom, TTo> Template { get; set; } = new();

    /// <summary>
    /// Initiaze properties on <typeparam name="T"></typeparam> to non-null values.
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Map(TFrom objFrom, TTo objTo)
        => CacheDel(objFrom, objTo);

    /// <summary>
    /// Initiaze properties on <typeparam name="T"></typeparam> to non-null values.
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TTo Map(TFrom objFrom)
    {
        var objTo = Activator.CreateInstance<TTo>();
        CacheDel(objFrom, objTo);
        return objTo;
    }

    /// <summary>
    /// Initiaze properties on <typeparam name="T"></typeparam> to non-null values.
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static KeyValuePair<TKey, TValue> Map<TKey, TValue>(KeyValuePair<TKey, TValue> objFrom)
    //{
    //    KeyValuePair<TKey, TValue> kvPair = new ();
    //    Mapper<KeyValuePair<TKey, TValue>
    //    return objTo;
    //}

    /// <summary>
    /// Initiaze properties on <typeparam name="T"></typeparam> to non-null values.
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //public static List<TTo> Map(IList<TFrom> objFrom)
    //{
    //    var pool = ArrayPool<TTo>.Shared;
    //    int cnt = 0, cntTotal = 0, size = 100;
    //    var list = new List<TTo>(objFrom.Count);
    //    var array = pool.Rent(size);

    //    while (cntTotal < objFrom.Count)
    //    {
    //        while (cnt < size)
    //        {
    //            var objTo = new TTo();
    //            CacheDel(objFrom[cnt], objTo);

    //            array[cnt] = objTo;
    //            cnt++;
    //            cntTotal++;
    //        }
    //        cnt = 0;
    //        list.AddRange(array);
    //        pool.Return(array);
    //        array = pool.Rent(size);
    //    }

    //    return list;
    //}
    /// <summary>
    /// Initiaze properties on <typeparam name="T"></typeparam> to non-null values.
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TTo> Map(IEnumerable<TFrom> objFrom)
    {
        int cnt = 0, cntTotal = 0, size = 100;
        Span<TFrom> spFrom = objFrom.ToArray();
        var list = new TTo[spFrom.Length];
        Span<TTo> spTo = list;

        while (cnt < spFrom.Length)
        {
                spTo[cnt] = Map(spFrom[cnt]);
                cnt++;
        }

        return list;// Map(objFrom.ToList());
    }
    /// <summary>
    /// Initiaze properties on <typeparam name="T"></typeparam> to non-null values.
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<TTo> Map2(IEnumerable<TFrom> objFrom)
    {
        var pool = ArrayPool<TTo>.Shared;
        int cnt = 0, size = 100;
        //var from = objFrom.ToArray();
        var list = new List<TTo>();
        //var array = pool.Rent(size);
        //MemoryMarshal.Cast<long, Vector<long>>(Values);
        var from = CollectionsMarshal.AsSpan(objFrom.ToList());

        foreach (var item in from)
        {
            list.Add(Map(item));
        }

        //for (int i = 0; i < from.Length; i++)
        //{
        //    if (cnt == size)
        //    {
        //        cnt = 0;
        //        list.AddRange(array[..size]);
        //        pool.Return(array);
        //        array = pool.Rent(size);
        //    }
        //    var objTo = new TTo();
        //    CacheDel(from[i], objTo);

        //    array[cnt] = objTo;
        //    cnt++;
        //}

        //if (cnt > 0)
        //{
        //    list.AddRange(array[..cnt]);
        //    pool.Return(array);
        //}

        return list;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void BuildDelegates()
    {
        var infos = _proxyType.GetMethod(nameof(Map));
        CacheDel = infos.CreateDelegate<MapperDelegate>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Compile()
    {
        _proxyTypeName = $"Mapper{typeof(TFrom).Name}.Map{typeof(TFrom).Name}";

        var result = Template.Generate();

        var references = new List<PortableExecutableReference> {
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Private_CoreLib),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Runtime),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Collections),
            MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(List<>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Dictionary<,>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Initializer<>).GetTypeInfo().Assembly.Location),
            MetadataReference.CreateFromFile(typeof(TFrom).GetTypeInfo().Assembly.Location)};
        
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