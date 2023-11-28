using System.Runtime.CompilerServices;

namespace Initialize.DelimitedParser;

/// <summary>
/// Parser for delimited binary data
/// </summary>
public ref struct DelimitedBinaryParser
{
    byte _delimiter;

    // line buffer
    Span<byte> _buffer;
    Span<byte> _bufferRemaining;

    int _bufferPosition = 0;

    // last column value
    Span<byte> _lastFieldValue;
    int _lastFieldIndex = 0;
    bool _complete = false;

    public Span<byte> LastFieldValue => this._lastFieldValue;
    public int LastFieldIndex => this._lastFieldIndex;

    public DelimitedBinaryParser(Span<byte> bytes, byte delimeter = 0x2C)
    {
		this._delimiter = delimeter;
		this._buffer = this._bufferRemaining = bytes;
    }

    /// <summary>
    /// Parse interator
    /// </summary>
     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> Next()
    {
        if(this._complete) 
            throw new IndexOutOfRangeException("There are no more fields to be parsed.");

		this._bufferPosition = this._bufferRemaining.IndexOf(this._delimiter); // next delimiter

        if ((this._complete = this._bufferPosition == -1))	return this._lastFieldValue = this._bufferRemaining; // return last

		this._lastFieldValue = this._bufferRemaining.Slice(0, this._bufferPosition); // parse field

		this._bufferRemaining  = this._bufferRemaining.Slice(++this._bufferPosition); // advance past delimter

		this._lastFieldIndex++; // track field for rewind
			
        return this._lastFieldValue;
    }

    public Span<byte> this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if(index < this._lastFieldIndex) this.Rewind(index);
			
            if(this._complete) 
                throw new IndexOutOfRangeException("There are no more fields to be parsed.");

			this._bufferPosition = this._bufferRemaining.IndexOf(this._delimiter); // next delimiter

            if ((this._complete = this._bufferPosition == -1))	return this._lastFieldValue = this._bufferRemaining; // return last

			this._lastFieldValue = this._bufferRemaining.Slice(0, this._bufferPosition); // parse field

			this._bufferRemaining  = this._bufferRemaining.Slice(++this._bufferPosition); // advance past delimter

			this._lastFieldIndex++; // track field for rewind
			
            return this._lastFieldValue;
        }
    }
	
    private void Rewind(int i )
    {
        throw new IndexOutOfRangeException();
    }
}