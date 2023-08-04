﻿using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Initialize.DelimitedParser;

/// <summary>
/// Output that encapsulates sync stack output (via SpanByte) and async heap output (via IMemoryOwner)
/// </summary>
public unsafe struct SpanByteAndMemory : IHeapConvertible
{
	/// <summary>
	/// Stack output as SpanByte
	/// </summary>
	public SpanByte SpanByte;

	/// <summary>
	/// Heap output as IMemoryOwner
	/// </summary>
	public IMemoryOwner<byte> Memory;

	/// <summary>
	/// Constructor using given SpanByte
	/// </summary>
	/// <param name="spanByte"></param>
	public SpanByteAndMemory(SpanByte spanByte)
	{
		if (spanByte.Serialized) throw new Exception("Cannot create new SpanByteAndMemory using serialized SpanByte");
		SpanByte = spanByte;
		Memory = default;
	}

	/// <summary>
	/// Constructor using SpanByte at given (fixed) pointer, of given length
	/// </summary>
	public SpanByteAndMemory(void* pointer, int length)
	{
		SpanByte = new SpanByte(length, (IntPtr)pointer);
		Memory = default;
	}

	/// <summary>
	/// Get length
	/// </summary>
	public int Length
	{
		get => SpanByte.Length;
		set => SpanByte.Length = value;
	}

	/// <summary>
	/// Constructor using given IMemoryOwner
	/// </summary>
	/// <param name="memory"></param>
	public SpanByteAndMemory(IMemoryOwner<byte> memory)
	{
		SpanByte = default;
		SpanByte.Invalid = true;
		Memory = memory;
	}

	/// <summary>
	/// Constructor using given IMemoryOwner and length
	/// </summary>
	/// <param name="memory"></param>
	/// <param name="length"></param>
	public SpanByteAndMemory(IMemoryOwner<byte> memory, int length)
	{
		SpanByte = default;
		SpanByte.Invalid = true;
		Memory = memory;
		SpanByte.Length = length;
	}

	/// <summary>
	/// View a fixed Span&lt;byte&gt; as a SpanByteAndMemory
	/// </summary>
	/// <param name="span"></param>
	/// <returns></returns>
	public static SpanByteAndMemory FromFixedSpan(Span<byte> span)
	{
		return new SpanByteAndMemory { SpanByte = SpanByte.FromFixedSpan(span) };
	}


	/// <summary>
	/// Convert to be used on heap (IMemoryOwner)
	/// </summary>
	public void ConvertToHeap() { SpanByte.Invalid = true; }

	/// <summary>
	/// Is it allocated as SpanByte (on stack)?
	/// </summary>
	public bool IsSpanByte => !SpanByte.Invalid;
}

/// <summary>
/// Whether type supports converting to heap (e.g., when operation goes pending)
/// </summary>
public interface IHeapConvertible
{
	/// <summary>
	/// Convert to heap
	/// </summary>
	public void ConvertToHeap();
}
/// <summary>
/// Represents a pinned variable length byte array that is viewable as a fixed (pinned) Span&lt;byte&gt;
/// Format: [4-byte (int) length of payload][[optional 8-byte metadata] payload bytes...]
/// First 2 bits of length are used as a mask for properties, so max payload length is 1GB
/// </summary>
[StructLayout(LayoutKind.Explicit, Pack = 4)]
public unsafe struct SpanByte
{
	// Byte #31 is used to denote unserialized (1) or serialized (0) data 
	const int kUnserializedBitMask = 1 << 31;
	// Byte #30 is used to denote extra metadata present (1) or absent (0) in payload
	const int kExtraMetadataBitMask = 1 << 30;
	// Mask for header
	const int kHeaderMask = 0x3 << 30;

	/// <summary>
	/// Length of the payload
	/// </summary>
	[FieldOffset(0)]
	private int length;

	/// <summary>
	/// Start of payload
	/// </summary>
	[FieldOffset(4)]
	private IntPtr payload;

	internal IntPtr Pointer => payload;

	/// <summary>
	/// Pointer to the beginning of payload, not including metadata if any
	/// </summary>
	public byte* ToPointer()
	{
		if (Serialized)
			return MetadataSize + (byte*)Unsafe.AsPointer(ref payload);
		else
			return MetadataSize + (byte*)payload;
	}

	/// <summary>
	/// Pointer to the beginning of payload, including metadata if any
	/// </summary>
	public byte* ToPointerWithMetadata()
	{
		if (Serialized)
			return (byte*)Unsafe.AsPointer(ref payload);
		else
			return (byte*)payload;
	}

	/// <summary>
	/// Length of payload, including metadata if any
	/// </summary>
	public int Length
	{
		get { return length & ~kHeaderMask; }
		set { length = (length & kHeaderMask) | value; }
	}

	/// <summary>
	/// Length of payload, not including metadata if any
	/// </summary>
	public int LengthWithoutMetadata => (length & ~kHeaderMask) - MetadataSize;

	/// <summary>
	/// Format of structure
	/// </summary>
	public bool Serialized => (length & kUnserializedBitMask) == 0;

	/// <summary>
	/// Total serialized size in bytes, including header and metadata if any
	/// </summary>
	public int TotalSize => sizeof(int) + Length;

	/// <summary>
	/// Size of metadata header, if any (returns 0 or 8)
	/// </summary>
	public int MetadataSize => (length & kExtraMetadataBitMask) >> (30 - 3);

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param name="length"></param>
	/// <param name="payload"></param>
	public SpanByte(int length, IntPtr payload)
	{
		Debug.Assert(length <= ~kHeaderMask);
		this.length = length | kUnserializedBitMask;
		this.payload = payload;
	}

	/// <summary>
	/// Extra metadata header
	/// </summary>
	public long ExtraMetadata
	{
		get
		{
			if (Serialized)
				return MetadataSize > 0 ? *(long*)Unsafe.AsPointer(ref payload) : 0;
			else
				return MetadataSize > 0 ? *(long*)payload : 0;
		}
		set
		{
			if (value > 0)
			{
				length |= kExtraMetadataBitMask;
				Debug.Assert(Length >= MetadataSize);
				if (Serialized)
					*(long*)Unsafe.AsPointer(ref payload) = value;
				else
					*(long*)payload = value;
			}
		}
	}

	/// <summary>
	/// Mark SpanByte as having 8-byte metadata in header of payload
	/// </summary>
	public void MarkExtraMetadata()
	{
		Debug.Assert(Length >= 8);
		length |= kExtraMetadataBitMask;
	}

	/// <summary>
	/// Unmark SpanByte as having 8-byte metadata in header of payload
	/// </summary>
	public void UnmarkExtraMetadata()
	{
		length &= ~kExtraMetadataBitMask;
	}

	/// <summary>
	/// Check or set struct as invalid
	/// </summary>
	public bool Invalid
	{
		get { return ((length & kUnserializedBitMask) != 0) && payload == IntPtr.Zero; }
		set
		{
			if (value)
			{
				length |= kUnserializedBitMask;
				payload = IntPtr.Zero;
			}
			else
			{
				if (Invalid) length = 0;
			}
		}
	}

	/// <summary>
	/// Get Span&lt;byte&gt; for this SpanByte's payload (excluding metadata if any)
	/// </summary>
	/// <returns></returns>
	public Span<byte> AsSpan()
	{
		if (Serialized)
			return new Span<byte>(MetadataSize + (byte*)Unsafe.AsPointer(ref payload), Length - MetadataSize);
		else
			return new Span<byte>(MetadataSize + (byte*)payload, Length - MetadataSize);
	}

	/// <summary>
	/// Get ReadOnlySpan&lt;byte&gt; for this SpanByte's payload (excluding metadata if any)
	/// </summary>
	/// <returns></returns>
	public ReadOnlySpan<byte> AsReadOnlySpan()
	{
		if (Serialized)
			return new ReadOnlySpan<byte>(MetadataSize + (byte*)Unsafe.AsPointer(ref payload), Length - MetadataSize);
		else
			return new ReadOnlySpan<byte>(MetadataSize + (byte*)payload, Length - MetadataSize);
	}

	/// <summary>
	/// Get Span&lt;byte&gt; for this SpanByte's payload (including metadata if any)
	/// </summary>
	/// <returns></returns>
	public Span<byte> AsSpanWithMetadata()
	{
		if (Serialized)
			return new Span<byte>((byte*)Unsafe.AsPointer(ref payload), Length);
		else
			return new Span<byte>((byte*)payload, Length);
	}

	/// <summary>
	/// Get ReadOnlySpan&lt;byte&gt; for this SpanByte's payload (including metadata if any)
	/// </summary>
	/// <returns></returns>
	public ReadOnlySpan<byte> AsReadOnlySpanWithMetadata()
	{
		if (Serialized)
			return new ReadOnlySpan<byte>((byte*)Unsafe.AsPointer(ref payload), Length);
		else
			return new ReadOnlySpan<byte>((byte*)payload, Length);
	}

	/// <summary>
	/// If SpanByte is in a serialized form, return a non-serialized SpanByte wrapper that points to the same payload.
	/// The resulting SpanByte is safe to heap-copy, as long as the underlying payload remains fixed.
	/// </summary>
	/// <returns></returns>
	public SpanByte Deserialize()
	{
		if (!Serialized) return this;
		return new SpanByte(Length - MetadataSize, (IntPtr)(MetadataSize + (byte*)Unsafe.AsPointer(ref payload)));
	}

	/// <summary>
	/// Reinterpret a fixed Span&lt;byte&gt; as a serialized SpanByte. Automatically adds Span length to the first 4 bytes.
	/// </summary>
	/// <param name="span"></param>
	/// <returns></returns>
	public static ref SpanByte Reinterpret(Span<byte> span)
	{
		Debug.Assert(span.Length - sizeof(int) <= ~kHeaderMask);

		fixed (byte* ptr = span)
		{
			*(int*)ptr = span.Length - sizeof(int);
			return ref Unsafe.AsRef<SpanByte>(ptr);
		}
	}

	/// <summary>
	/// Reinterpret a fixed ReadOnlySpan&lt;byte&gt; as a serialized SpanByte, without adding length header
	/// </summary>
	/// <param name="span"></param>
	/// <returns></returns>
	public static ref SpanByte ReinterpretWithoutLength(ReadOnlySpan<byte> span)
	{
		fixed (byte* ptr = span)
		{
			return ref Unsafe.AsRef<SpanByte>(ptr);
		}
	}

	/// <summary>
	/// Reinterpret a fixed pointer as a serialized SpanByte
	/// </summary>
	/// <param name="ptr"></param>
	/// <returns></returns>
	public static ref SpanByte Reinterpret(byte* ptr)
	{
		return ref Unsafe.AsRef<SpanByte>(ptr);
	}

	/// <summary>
	/// Reinterpret a fixed ref as a serialized SpanByte (user needs to write the payload length to the first 4 bytes)
	/// </summary>
	/// <returns></returns>
	public static ref SpanByte Reinterpret<T>(ref T t)
	{
		return ref Unsafe.As<T, SpanByte>(ref t);
	}

	/// <summary>
	/// Create a SpanByte around a fixed Span&lt;byte&gt;. Warning: ensure the Span is fixed until operation returns.
	/// </summary>
	/// <param name="span"></param>
	/// <returns></returns>
	public static SpanByte FromFixedSpan(Span<byte> span)
	{
		return new SpanByte(span.Length, (IntPtr)Unsafe.AsPointer(ref span[0]));
	}

	/// <summary>
	/// Create a SpanByte around a fixed ReadOnlySpan&lt;byte&gt;. Warning: ensure the Span is fixed until operation returns.
	/// </summary>
	/// <param name="span"></param>
	/// <returns></returns>
	public static SpanByte FromFixedSpan(ReadOnlySpan<byte> span)
	{
		fixed (byte* ptr = span)
		{
			return new SpanByte(span.Length, (IntPtr)ptr);
		}
	}

	/// <summary>
	/// Create SpanByte around a pinned Memory&lt;byte&gt;. Warning: ensure the Memory is pinned until operation returns.
	/// </summary>
	/// <param name="memory"></param>
	/// <returns></returns>
	public static SpanByte FromPinnedMemory(Memory<byte> memory)
	{
		return FromFixedSpan(memory.Span);
	}

	/// <summary>
	/// Create a SpanByte around a pinned memory pointer of given length
	/// </summary>
	/// <param name="ptr"></param>
	/// <param name="length"></param>
	/// <returns></returns>
	public static SpanByte FromPointer(byte* ptr, int length)
	{
		return new SpanByte(length, (IntPtr)ptr);
	}


	/// <summary>
	/// Convert payload to new byte array
	/// </summary>
	public byte[] ToByteArray()
	{
		return AsReadOnlySpan().ToArray();
	}

	/// <summary>
	/// Convert payload to specified (disposable) memory owner
	/// </summary>
	public (IMemoryOwner<byte> memory, int length) ToMemoryOwner(MemoryPool<byte> pool)
	{
		var dst = pool.Rent(Length);
		AsReadOnlySpan().CopyTo(dst.Memory.Span);
		return (dst, Length);
	}

	/// <summary>
	/// Convert to SpanByteAndMemory wrapper
	/// </summary>
	/// <returns></returns>
	public SpanByteAndMemory ToSpanByteAndMemory()
	{
		return new SpanByteAndMemory(this);
	}

	/// <summary>
	/// Try to copy to given pre-allocated SpanByte, checking if space permits at destination SpanByte
	/// </summary>
	/// <param name="dst"></param>
	public bool TryCopyTo(ref SpanByte dst)
	{
		if (dst.Length < Length) return false;
		CopyTo(ref dst);
		return true;
	}

	/// <summary>
	/// Blindly copy to given pre-allocated SpanByte, assuming sufficient space.
	/// Does not change length of destination.
	/// </summary>
	/// <param name="dst"></param>
	public void CopyTo(ref SpanByte dst)
	{
		dst.UnmarkExtraMetadata();
		dst.ExtraMetadata = ExtraMetadata;
		AsReadOnlySpan().CopyTo(dst.AsSpan());
	}

	/// <summary>
	/// Shrink the length header of the in-place allocated buffer on
	/// FASTER hybrid log, pointed to by the given SpanByte.
	/// Zeroes out the extra space to retain log scan correctness.
	/// </summary>
	/// <param name="newLength">New length of payload (including metadata)</param>
	/// <returns></returns>
	public bool ShrinkSerializedLength(int newLength)
	{
		if (newLength > Length) return false;

		// Zero-fill extra space - needed so log scan does not see spurious data
		if (newLength < Length)
		{
			AsSpanWithMetadata().Slice(newLength).Clear();
			Length = newLength;
		}
		return true;
	}

	/// <summary>
	/// Copy to given SpanByteAndMemory (only payload copied to actual span/memory)
	/// </summary>
	/// <param name="dst"></param>
	/// <param name="memoryPool"></param>
	public void CopyTo(ref SpanByteAndMemory dst, MemoryPool<byte> memoryPool)
	{
		if (dst.IsSpanByte)
		{
			if (dst.Length >= Length)
			{
				dst.Length = Length;
				AsReadOnlySpan().CopyTo(dst.SpanByte.AsSpan());
				return;
			}
			dst.ConvertToHeap();
		}

		dst.Memory = memoryPool.Rent(Length);
		dst.Length = Length;
		AsReadOnlySpan().CopyTo(dst.Memory.Memory.Span);
	}

	/// <summary>
	/// Copy to given SpanByteAndMemory (header and payload copied to actual span/memory)
	/// </summary>
	/// <param name="dst"></param>
	/// <param name="memoryPool"></param>
	public void CopyWithHeaderTo(ref SpanByteAndMemory dst, MemoryPool<byte> memoryPool)
	{
		if (dst.IsSpanByte)
		{
			if (dst.Length >= TotalSize)
			{
				dst.Length = TotalSize;
				var span = dst.SpanByte.AsSpan();
				fixed (byte* ptr = span)
					*(int*)ptr = Length;
				dst.SpanByte.ExtraMetadata = ExtraMetadata;

				AsReadOnlySpan().CopyTo(span.Slice(sizeof(int) + MetadataSize));
				return;
			}
			dst.ConvertToHeap();
		}

		dst.Memory = memoryPool.Rent(TotalSize);
		dst.Length = TotalSize;
		fixed (byte* ptr = dst.Memory.Memory.Span)
			*(int*)ptr = Length;
		dst.SpanByte.ExtraMetadata = ExtraMetadata;
		AsReadOnlySpan().CopyTo(dst.Memory.Memory.Span.Slice(sizeof(int) + MetadataSize));
	}

	/// <summary>
	/// Copy serialized version to specified memory location
	/// </summary>
	/// <param name="destination"></param>
	public void CopyTo(byte* destination)
	{
		if (Serialized)
		{
			*(int*)destination = length;
			Buffer.MemoryCopy(Unsafe.AsPointer(ref payload), destination + sizeof(int), Length, Length);
		}
		else
		{
			*(int*)destination = length & ~kUnserializedBitMask;
			Buffer.MemoryCopy((void*)payload, destination + sizeof(int), Length, Length);
		}
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		var bytes = AsSpan();
		var len = Math.Min(this.Length, 8);
		StringBuilder sb = new();
		for (var ii = 0; ii < len; ++ii)
			sb.Append(bytes[ii].ToString("x2"));
		if (bytes.Length > len)
			sb.Append("...");
		return sb.ToString();
	}
}