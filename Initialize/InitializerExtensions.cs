namespace Initialize;

public static class InitializerExtensions
{
    public static T InitValueOrDefault<T>(this T? value) where T : unmanaged => ((T?)value).GetValueOrDefault();
    public static T InitValueOrDefault<T>(this T value) where T : unmanaged => default(T);
}