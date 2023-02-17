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
    public string MemoryByteToString(int index) => $"{{0}} [{index}].Span.ToString()";
    public string MemoryByteToStringNullIfEmpty(int index) => $"{{0}} [{index}].Span.ToStringNullIfEmpty()";
    public string MemoryByteToIntNullable(int index) => $"{{0}} [{index}].Span.ToNullableInt()";
    public string MemoryByteToInt(int index) => $"{{0}} [{index}].Span.ToInt()";
    public string MemoryByteToLongNullable(int index) => $"{{0}} [{index}].Span.ToNullableLong()";
    public string MemoryByteToLong(int index) => $"{{0}} [{index}].Span.ToLong()";
    public string MemoryByteToDoubleNullable(int index) => $"{{0}} [{index}].Span.ToNullableDouble()";
    public string MemoryByteToDouble(int index) => $"{{0}} [{index}].Span.ToDouble()";
    public string MemoryByteToDateTimeNullable(int index) => $"{{0}} [{index}].Span.ToNullableDateTime()";
    public string MemoryByteToDateTimeNullable(int index, string parseFormat) => $"{{0}} [{index}].Span.ToNullableDateTime({parseFormat})";
    public string MemoryByteToDateTime(int index) => $"{{0}} [{index}].Span.ToDateTime()";
    public string MemoryByteToTimeSpanNullable(int index) => $"{{0}} [{index}].Span.ToNullableTimeSpan()";
    public string MemoryByteToTimeSpan(int index) => $"{{0}} [{index}].Span.ToTimeSpan()";
}

public static class ParseExtensions
{
    public static string ToString(this Span<byte> span)
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
    //    if (Utf8Parser.TryParse(span, out DateTime value, out int bytesConsumed))
    //        return value;
    //    return null;
    //}
    
    public static DateTime? ToNullableDateTime(this Span<byte> span)
    {
        var dtString = Encoding.UTF8.GetString(span);
        if (DateTime.TryParse(dtString, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return null;
    }
    public static DateTime? ToNullableDateTime(this Span<byte> span, string format)
    {
        var dtString = Encoding.UTF8.GetString(span);
        if (DateTime.TryParseExact(dtString, format, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return null;
    }

    public static DateTime ToDateTime(this Span<byte> span, string format)
    {
        var dtString = Encoding.UTF8.GetString(span);
        if (DateTime.TryParseExact(dtString, format, null, DateTimeStyles.AssumeLocal, out var value))
            return value;
        return default;
    }

    public static TimeSpan? ToNullableTimeSpan(this Span<byte> span)
    {
        var dtString = Encoding.UTF8.GetString(span);
        if (TimeSpan.TryParse(dtString, out var d))
            return d;
        return null;
    }
    public static TimeSpan ToTimeSpan(this Span<byte> span)
    {
        var dtString = Encoding.UTF8.GetString(span);
        if (TimeSpan.TryParse(dtString, out var d))
            return d;
        return default;
    }
    public static char? ToNullableChar(this Span<byte> span)
    {
        var dtString = Encoding.UTF8.GetString(span);
        if (Char.TryParse(dtString, out char value))
            return value;
        return null;
    }
}
