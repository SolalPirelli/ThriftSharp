using ThriftSharp.Internals;

namespace ThriftSharp.Protocols
{
    /// <summary>
    /// Represents a Thrift protocol that can send and receive primitive types as well as containers, structs and messages.
    /// </summary>
    internal interface IThriftProtocol
    {
        /// <summary>
        /// Reads a message header.
        /// </summary>
        /// <returns>A message header.</returns>
        ThriftMessageHeader ReadMessageHeader();

        /// <summary>
        /// Reads an end-of-message token.
        /// </summary>
        void ReadMessageEnd();

        /// <summary>
        /// Reads a struct header.
        /// </summary>
        /// <returns>A struct header.</returns>
        ThriftStructHeader ReadStructHeader();

        /// <summary>
        /// Reads an end-of-struct token.
        /// </summary>
        void ReadStructEnd();

        /// <summary>
        /// Reads a field header.
        /// </summary>
        /// <returns>
        /// A field header, or null if an end-of-field token was encountered.
        /// </returns>
        ThriftFieldHeader ReadFieldHeader();

        /// <summary>
        /// Reads an end-of-field token.
        /// </summary>
        void ReadFieldEnd();

        /// <summary>
        /// Reads a list header.
        /// </summary>
        /// <returns>A list header.</returns>
        ThriftCollectionHeader ReadListHeader();

        /// <summary>
        /// Reads an end-of-list token.
        /// </summary>
        void ReadListEnd();

        /// <summary>
        /// Reads a set header.
        /// </summary>
        /// <returns>The set header.</returns>
        ThriftCollectionHeader ReadSetHeader();

        /// <summary>
        /// Reads an end-of-set token.
        /// </summary>
        void ReadSetEnd();

        /// <summary>
        /// Reads a map header.
        /// </summary>
        /// <returns>A map header.</returns>
        ThriftMapHeader ReadMapHeader();

        /// <summary>
        /// Reads an end-of-map token.
        /// </summary>
        void ReadMapEnd();

        /// <summary>
        /// Reads a boolean value.
        /// </summary>
        /// <returns>A boolean value.</returns>
        bool ReadBoolean();

        /// <summary>
        /// Reads a signed byte.
        /// </summary>
        /// <returns>A signed byte.</returns>
        sbyte ReadSByte();

        /// <summary>
        /// Reads an IEEE 754 double-precision floating-point number.
        /// </summary>
        /// <returns>An IEEE 754 double-precision floating-point number.</returns>
        double ReadDouble();

        /// <summary>
        /// Reads a 16-bit integer.
        /// </summary>
        /// <returns>A 16-bit integer.</returns>
        short ReadInt16();

        /// <summary>
        /// Reads a 32-bit integer.
        /// </summary>
        /// <returns>A 32-bit integer.</returns>
        int ReadInt32();

        /// <summary>
        /// Reads a 64-bit integer.
        /// </summary>
        /// <returns>A 64-bit integer.</returns>
        long ReadInt64();

        /// <summary>
        /// Reads a string.
        /// </summary>
        /// <returns>A string.</returns>
        string ReadString();

        /// <summary>
        /// Reads an array of signed bytes.
        /// </summary>
        /// <returns>An array of signed bytes.</returns>
        sbyte[] ReadBinary();


        /// <summary>
        /// Writes the specified message header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteMessageHeader( ThriftMessageHeader header );

        /// <summary>
        /// Writes an end-of-message token.
        /// </summary>
        void WriteMessageEnd();

        /// <summary>
        /// Writes the specified struct header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteStructHeader( ThriftStructHeader header );

        /// <summary>
        /// Writes an end-of-struct token.
        /// </summary>
        void WriteStructEnd();

        /// <summary>
        /// Writes the specified field header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteFieldHeader( ThriftFieldHeader header );

        /// <summary>
        /// Writes an end-of-field token.
        /// </summary>
        void WriteFieldEnd();

        /// <summary>
        /// Writes a token signaling the end of fields in a struct.
        /// </summary>
        void WriteFieldStop();

        /// <summary>
        /// Writes the specified list header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteListHeader( ThriftCollectionHeader header );

        /// <summary>
        /// Writes an end-of-list token.
        /// </summary>
        void WriteListEnd();

        /// <summary>
        /// Writes the specified set header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteSetHeader( ThriftCollectionHeader header );

        /// <summary>
        /// Writes an end-of-set token.
        /// </summary>
        void WriteSetEnd();

        /// <summary>
        /// Writes the specified map header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteMapHeader( ThriftMapHeader header );

        /// <summary>
        /// Writes an end-of-map token.
        /// </summary>
        void WriteMapEnd();

        /// <summary>
        /// Writes the specified boolean value.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        void WriteBoolean( bool value );

        /// <summary>
        /// Writes the specified signed byte.
        /// </summary>
        /// <param name="value">The signed byte value.</param>
        void WriteSByte( sbyte value );

        /// <summary>
        /// Writes the specified IEEE 754 double-precision floating-point number.
        /// </summary>
        /// <param name="value">The floating point value.</param>
        void WriteDouble( double value );

        /// <summary>
        /// Writes the specified 16-bit signed integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        void WriteInt16( short value );

        /// <summary>
        /// Writes the specified 32-bit signed integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        void WriteInt32( int value );

        /// <summary>
        /// Writes the specified 64-bit signed integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        void WriteInt64( long value );

        /// <summary>
        /// Writes the specified string.
        /// </summary>
        /// <param name="value">The string.</param>
        void WriteString( string value );

        /// <summary>
        /// Writes the specified array of signed bytes.
        /// </summary>
        /// <param name="bytes">The array of signed bytes.</param>
        void WriteBinary( sbyte[] bytes );
    }
}