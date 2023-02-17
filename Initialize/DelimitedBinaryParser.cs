using System.Runtime.CompilerServices;

namespace Initialize;

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

    public Span<byte> LastFieldValue => _lastFieldValue;
    public int LastFieldIndex => _lastFieldIndex;

    public DelimitedBinaryParser(Span<byte> bytes, byte delimeter = 0x2C)
    {
        _delimiter = delimeter;
        _buffer = _bufferRemaining = bytes;
    }

    /// <summary>
    /// Parse interator
    /// </summary>
     [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> Next()
    {
        if(_complete) 
            throw new IndexOutOfRangeException("There are no more fields to be parsed.");
			
        _bufferPosition = _bufferRemaining.IndexOf(_delimiter); // next delimiter

        if ((_complete =_bufferPosition == -1))	return _lastFieldValue = _bufferRemaining; // return last
			 
        _lastFieldValue = _bufferRemaining.Slice(0, _bufferPosition); // parse field
			 
        _bufferRemaining  = _bufferRemaining.Slice(++_bufferPosition); // advance past delimter
			
        _lastFieldIndex++; // track field for rewind
			
        return _lastFieldValue;
    }

    public Span<byte> this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if(index < _lastFieldIndex) Rewind(index);
			
            if(_complete) 
                throw new IndexOutOfRangeException("There are no more fields to be parsed.");
			
            _bufferPosition = _bufferRemaining.IndexOf(_delimiter); // next delimiter

            if ((_complete =_bufferPosition == -1))	return _lastFieldValue = _bufferRemaining; // return last
			 
            _lastFieldValue = _bufferRemaining.Slice(0, _bufferPosition); // parse field
			 
            _bufferRemaining  = _bufferRemaining.Slice(++_bufferPosition); // advance past delimter
			
            _lastFieldIndex++; // track field for rewind
			
            return _lastFieldValue;
        }
    }
	
    private void Rewind(int i )
    {
        throw new IndexOutOfRangeException();
    }
}