using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Initialize;

public class ParseUTF8
{
    public string ToString(int index) => $"{{0}} [{index}].ToUtf8String()";
    public string ToStringNullIfEmpty(int index) => $"{{0}} [{index}].ToStringNullIfEmpty()";
    public string ToIntNullable(int index) => $"{{0}} [{index}].ToNullableInt()";
    public string ToInt(int index) => $"{{0}} [{index}].ToInt()";
    public string ToLongNullable(int index) => $"{{0}} [{index}].ToNullableLong()";
    public string ToLong(int index) => $"{{0}} [{index}].ToLong()";
    public string ToDoubleNullable(int index) => $"{{0}} [{index}].ToNullableDouble()";
    public string ToDouble(int index) => $"{{0}} [{index}].ToDouble()";
    public string ToDateTimeNullable(int index) => $"{{0}} [{index}].ToNullableDateTime()";
    public string ToDateTimeNullable(int index, string parseFormat) => $"{{0}} [{index}].ToNullableDateTime({parseFormat})";
    public string ToDateTime(int index) => $"{{0}} [{index}].ToDateTime()";
    public string ToDateTime(int index, string parseFormat) => $"{{0}} [{index}].ToDateTime({parseFormat})";
    public string ToTimeSpanNullable(int index) => $"{{0}} [{index}].ToNullableTimeSpan()";
    public string ToTimeSpan(int index) => $"{{0}} [{index}].ToTimeSpan()";
}

public static partial class ParseExtensions
{
     public static string ToUtf8String(this Memory<byte> span)
    {
        return Encoding.UTF8.GetString(span.Span);
    }

    public static string ToStringNullIfEmpty(this Memory<byte> span)
    {
        if (span.Length > 0)
            return Encoding.UTF8.GetString(span.Span);
        return null;
    }

    public static double ToDouble(this Memory<byte> span)
    {
        if (Utf8Parser.TryParse(span.Span, out double value, out int bytesConsumed))
            return value;
        return default;
    }

    public static double? ToNullableDouble(this Memory<byte> span)
    {
        if (Utf8Parser.TryParse(span.Span, out double value, out int bytesConsumed))
            return value;
        return null;
    }

    public static int ToInt(this Memory<byte> span)
    {
        if (Utf8Parser.TryParse(span.Span, out int value, out int bytesConsumed))
            return value;
        return default;
    }

    public static int? ToNullableInt(this Memory<byte> span)
    {
        if (Utf8Parser.TryParse(span.Span, out int value, out int bytesConsumed))
            return value;
        return null;
    }

    public static long ToLong(this Memory<byte> span)
    {
        if (Utf8Parser.TryParse(span.Span, out long value, out int bytesConsumed))
            return value;
        return default;
    }

    public static long? ToNullableLong(this Memory<byte> span)
    {
        if (Utf8Parser.TryParse(span.Span, out long value, out int bytesConsumed))
            return value;
        return null;
    }

    ///// <summary>
    ///// https://learn.microsoft.com/en-us/dotnet/api/system.buffers.text.utf8parser.tryparse?view=net-7.0#system-buffers-text-utf8parser-tryparse(system-readonlyspan((system-byte))-system-datetime@-system-int32@-system-char)
    ///// </summary>
    //public static DateTime? ToNullableDateTime(this Memory<byte> span)
    //{
    //    if (Utf8Parser.TryParse(span.Span, out DateTime value, out int bytesConsumed))
    //        return value;
    //    return null;
    //}
    
    public static DateTime? ToNullableDateTime(this Memory<byte> span)
    {
        var dtString = span.ToUtf8String();
        if (DateTime.TryParse(dtString, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return null;
    }
    public static DateTime? ToNullableDateTime(this Memory<byte> span, string format)
    {
        var dtString = span.ToUtf8String();
        if (DateTime.TryParseExact(dtString, format, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return null;
    }

    public static DateTime ToDateTime(this Memory<byte> span, string format)
    {
        var dtString = span.ToUtf8String();
        if (DateTime.TryParseExact(dtString, format, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return default;
    }

    public static TimeSpan? ToNullableTimeSpan(this Memory<byte> span)
    {
        var dtString = span.ToUtf8String();
        if (TimeSpan.TryParse(dtString, out var d))
            return d;
        return null;
    }
    public static TimeSpan ToTimeSpan(this Memory<byte> span)
    {
        var dtString = span.ToUtf8String();
        if (TimeSpan.TryParse(dtString, out var d))
            return d;
        return default;
    }
    public static char? ToNullableChar(this Memory<byte> span)
    {
        var dtString = span.ToUtf8String();
        if (Char.TryParse(dtString, out char value))
            return value;
        return null;
    }
}

public static partial class ParseExtensions
{
    public static string ToUtf8String(this Span<byte> span)
    {
        return Encoding.UTF8.GetString(span);
    }

    public static string ToStringNullIfEmpty(this Span<byte> span)
    {
        if (span.Length > 0)
            return Encoding.UTF8.GetString(span);
        return null;
    }

    public static double ToDouble(this Span<byte> span)
    {
        if (Utf8Parser.TryParse(span, out double value, out int bytesConsumed))
            return value;
        return default;
    }

    public static double? ToNullableDouble(this Span<byte> span)
    {
        if (Utf8Parser.TryParse(span, out double value, out int bytesConsumed))
            return value;
        return null;
    }

    public static int ToInt(this Span<byte> span)
    {
        if (Utf8Parser.TryParse(span, out int value, out int bytesConsumed))
            return value;
        return default;
    }

    public static int? ToNullableInt(this Span<byte> span)
    {
        if (Utf8Parser.TryParse(span, out int value, out int bytesConsumed))
            return value;
        return null;
    }

    public static long ToLong(this Span<byte> span)
    {
        if (Utf8Parser.TryParse(span, out long value, out int bytesConsumed))
            return value;
        return default;
    }

    public static long? ToNullableLong(this Span<byte> span)
    {
        if (Utf8Parser.TryParse(span, out long value, out int bytesConsumed))
            return value;
        return null;
    }

    ///// <summary>
    ///// https://learn.microsoft.com/en-us/dotnet/api/system.buffers.text.utf8parser.tryparse?view=net-7.0#system-buffers-text-utf8parser-tryparse(system-readonlyspan((system-byte))-system-datetime@-system-int32@-system-char)
    ///// </summary>
    //public static DateTime? ToNullableDateTime(this Span<byte> span)
    //{
    //    if (Utf8Parser.TryParse(span.Span, out DateTime value, out int bytesConsumed))
    //        return value;
    //    return null;
    //}
    
    public static DateTime? ToNullableDateTime(this Span<byte> span)
    {
        var dtString = span.ToUtf8String();
        if (DateTime.TryParse(dtString, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return null;
    }
    public static DateTime? ToNullableDateTime(this Span<byte> span, string format)
    {
        var dtString = span.ToUtf8String();
        if (DateTime.TryParseExact(dtString, format, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return null;
    }

    public static DateTime ToDateTime(this Span<byte> span, string format)
    {
        var dtString = span.ToUtf8String();
        if (DateTime.TryParseExact(dtString, format, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return default;
    }

    public static TimeSpan? ToNullableTimeSpan(this Span<byte> span)
    {
        var dtString = span.ToUtf8String();
        if (TimeSpan.TryParse(dtString, out var d))
            return d;
        return null;
    }
    public static TimeSpan ToTimeSpan(this Span<byte> span)
    {
        var dtString = span.ToUtf8String();
        if (TimeSpan.TryParse(dtString, out var d))
            return d;
        return default;
    }
    public static char? ToNullableChar(this Span<byte> span)
    {
        var dtString = span.ToUtf8String();
        if (Char.TryParse(dtString, out char value))
            return value;
        return null;
    }
}
public static partial class ParseExtensions
{
    public static string ToUtf8String(this byte[] span)
    {
        return Encoding.UTF8.GetString(span);
    }

    public static string ToStringNullIfEmpty(this byte[] span)
    {
        if (span.Length > 0)
            return Encoding.UTF8.GetString(span);
        return null;
    }

    public static double ToDouble(this byte[] span)
    {
        if (Utf8Parser.TryParse(span, out double value, out int bytesConsumed))
            return value;
        return default;
    }

    public static double? ToNullableDouble(this byte[] span)
    {
        if (Utf8Parser.TryParse(span, out double value, out int bytesConsumed))
            return value;
        return null;
    }

    public static int ToInt(this byte[] span)
    {
        if (Utf8Parser.TryParse(span, out int value, out int bytesConsumed))
            return value;
        return default;
    }

    public static int? ToNullableInt(this byte[] span)
    {
        if (Utf8Parser.TryParse(span, out int value, out int bytesConsumed))
            return value;
        return null;
    }

    public static long ToLong(this byte[] span)
    {
        if (Utf8Parser.TryParse(span, out long value, out int bytesConsumed))
            return value;
        return default;
    }

    public static long? ToNullableLong(this byte[] span)
    {
        if (Utf8Parser.TryParse(span, out long value, out int bytesConsumed))
            return value;
        return null;
    }

    ///// <summary>
    ///// https://learn.microsoft.com/en-us/dotnet/api/system.buffers.text.utf8parser.tryparse?view=net-7.0#system-buffers-text-utf8parser-tryparse(system-readonlyspan((system-byte))-system-datetime@-system-int32@-system-char)
    ///// </summary>
    //public static DateTime? ToNullableDateTime(this byte[] span)
    //{
    //    if (Utf8Parser.TryParse(span.Span, out DateTime value, out int bytesConsumed))
    //        return value;
    //    return null;
    //}
    
    public static DateTime? ToNullableDateTime(this byte[] span)
    {
        var dtString = span.ToUtf8String();
        if (DateTime.TryParse(dtString, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return null;
    }
    public static DateTime? ToNullableDateTime(this byte[] span, string format)
    {
        var dtString = span.ToUtf8String();
        if (DateTime.TryParseExact(dtString, format, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return null;
    }

    public static DateTime ToDateTime(this byte[] span, string format)
    {
        var dtString = span.ToUtf8String();
        if (DateTime.TryParseExact(dtString, format, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return default;
    }

    public static TimeSpan? ToNullableTimeSpan(this byte[] span)
    {
        var dtString = span.ToUtf8String();
        if (TimeSpan.TryParse(dtString, out var d))
            return d;
        return null;
    }
    public static TimeSpan ToTimeSpan(this byte[] span)
    {
        var dtString = span.ToUtf8String();
        if (TimeSpan.TryParse(dtString, out var d))
            return d;
        return default;
    }
    public static char? ToNullableChar(this byte[] span)
    {
        var dtString = span.ToUtf8String();
        if (Char.TryParse(dtString, out char value))
            return value;
        return null;
    }
}