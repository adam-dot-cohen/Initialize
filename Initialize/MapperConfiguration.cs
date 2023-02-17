using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Initialize;
/// <summary>
/// Creates a map from <typeparamref name="TFrom"/> to <typeparamref name="TTo"/>
/// </summary>
/// <typeparam name="TFrom"></typeparam>
/// <typeparam name="TTo"></typeparam>
public static class MapperConfiguration<TFrom, TTo>
{
    static MapperConfiguration() => Template = new MapperDefaultTemplate<TFrom, TTo>();
    internal static IMapperTemplate<TFrom, TTo> Template { get; private set; }

    public static void Configure(Action<MapperManualTemplate<TFrom, TTo>> config)
    {
        var template = new MapperManualTemplate<TFrom, TTo>();

        config(template);

        Template = template;
    }
}
