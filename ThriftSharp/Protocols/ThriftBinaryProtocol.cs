// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Text;
using System.Threading.Tasks;
using ThriftSharp.Internals;
using ThriftSharp.Transport;

namespace ThriftSharp.Protocols
{
    /// <summary>
    /// Thrift's "binary" protocol. A compact but simple way to represent Thrift data.
    /// </summary>
    internal sealed class ThriftBinaryProtocol : IThriftProtocol
    {
        // The current Thrift protocol version.
        private const uint Version1 = 0x80010000;
        // A mask used to store more information in the message size field.
        private const uint VersionMask = 0xffff0000;

        // PERF: Cached buffers for reading/writing 1-, 2-, 4- and 8-byte elements
        private readonly byte[] buffer1 = new byte[1], buffer2 = new byte[2], buffer4 = new byte[4], buffer8 = new byte[8];

        private readonly IThriftTransport _transport;

        /// <summary>
        /// Initializes a new instance of the ThriftBinaryProtocol class using the specified binary transport.
        /// </summary>
        /// <param name="transport">The transport used to transmit data.</param>
        public ThriftBinaryProtocol( IThriftTransport transport )
        {
            _transport = transport;
        }


        /// <summary>
        /// Reads a message header.
        /// </summary>
        public ThriftMessageHeader ReadMessageHeader()
        {
            int size = ReadInt32();
            if ( size < 0 )
            {
                uint version = (uint) size & VersionMask;
                if ( version != Version1 )
                {
                    throw new ThriftProtocolException( ThriftProtocolExceptionType.InvalidProtocol );
                }

                var type = (ThriftMessageType) ( size & 0xFF );
                string name = ReadString();
                int id = ReadInt32();
                return new ThriftMessageHeader( id, name, type );
            }
            else
            {
                // Old protocol version
                byte[] nameBytes = new byte[size];
                _transport.ReadBytes( nameBytes );
                string name = Encoding.UTF8.GetString( nameBytes, 0, nameBytes.Length );

                _transport.ReadBytes( buffer1 );
                var type = (ThriftMessageType) buffer1[0];

                int id = ReadInt32();
                return new ThriftMessageHeader( id, name, type );
            }
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void ReadMessageEnd() { }

        /// <summary>
        /// Returns an empty struct header.
        /// The binary protocol does not store struct headers.
        /// </summary>
        public ThriftStructHeader ReadStructHeader()
        {
            return new ThriftStructHeader( "" );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void ReadStructEnd() { }

        /// <summary>
        /// Reads a field header, or returns null if an end-of-field token was encountered.
        /// </summary>
        public ThriftFieldHeader ReadFieldHeader()
        {
            _transport.ReadBytes( buffer1 );
            if ( buffer1[0] == ThriftFieldHeader.Stop )
            {
                return null;
            }

            short id = ReadInt16();
            return new ThriftFieldHeader( id, "", (ThriftTypeId) buffer1[0] );
        }

        /// <summary>
        /// Does nothing.
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void ReadFieldEnd() { }

        /// <summary>
        /// Reads a list header.
        /// </summary>
        public ThriftCollectionHeader ReadListHeader()
        {
            _transport.ReadBytes( buffer1 );
            int count = ReadInt32();
            return new ThriftCollectionHeader( count, (ThriftTypeId) buffer1[0] );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void ReadListEnd() { }

        /// <summary>
        /// Reads a set header.
        /// </summary>
        public ThriftCollectionHeader ReadSetHeader()
        {
            _transport.ReadBytes( buffer1 );
            int count = ReadInt32();
            return new ThriftCollectionHeader( count, (ThriftTypeId) buffer1[0] );
        }

        /// <summary>
        /// Does nothing.
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void ReadSetEnd() { }

        /// <summary>
        /// Reads a map header.
        /// </summary>
        public ThriftMapHeader ReadMapHeader()
        {
            _transport.ReadBytes( buffer2 );
            int count = ReadInt32();
            return new ThriftMapHeader( count, (ThriftTypeId) buffer2[0], (ThriftTypeId) buffer2[1] );
        }

        /// <summary>
        /// Does nothing.
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void ReadMapEnd() { }

        /// <summary>
        /// Reads a boolean value as a byte, where 0 is true and anything else is false.
        /// </summary>
        public bool ReadBoolean()
        {
            _transport.ReadBytes( buffer1 );
            return buffer1[0] != 0;
        }

        /// <summary>
        /// Reads a signed byte.
        /// </summary>
        public sbyte ReadSByte()
        {
            _transport.ReadBytes( buffer1 );
            return (sbyte) buffer1[0];
        }

        /// <summary>
        /// Reads a big-endian, IEEE-754 double-precision floating-point number.
        /// </summary>
        public double ReadDouble()
        {
            return BitConverter.Int64BitsToDouble( ReadInt64() );
        }

        /// <summary>
        /// Reads a big-endian 16-bit integer.
        /// </summary>
        public short ReadInt16()
        {
            _transport.ReadBytes( buffer2 );
            return (short) ( ( buffer2[0] << 8 ) | ( buffer2[1] ) );
        }

        /// <summary>
        /// Reads a big-endian 32-bit integer.
        /// </summary>
        public int ReadInt32()
        {
            _transport.ReadBytes( buffer4 );
            return ( buffer4[0] << 24 ) | ( buffer4[1] << 16 ) | ( buffer4[2] << 8 ) | ( buffer4[3] );
        }

        /// <summary>
        /// Reads a big-endian 64-bit integer.
        /// </summary>
        public long ReadInt64()
        {
            _transport.ReadBytes( buffer8 );
            unchecked
            {
                return ( (long) buffer8[0] << 56 ) |
                       ( (long) buffer8[1] << 48 ) |
                       ( (long) buffer8[2] << 40 ) |
                       ( (long) buffer8[3] << 32 ) |
                       ( (long) buffer8[4] << 24 ) |
                       ( (long) buffer8[5] << 16 ) |
                       ( (long) buffer8[6] << 8 ) |
                       ( (long) buffer8[7] );
            }
        }

        /// <summary>
        /// Reads an UTF-8 string, whose length is a leading 32-bit integer.
        /// </summary>
        public string ReadString()
        {
            int length = ReadInt32();
            byte[] bytes = new byte[length];
            _transport.ReadBytes( bytes );
            return Encoding.UTF8.GetString( bytes, 0, bytes.Length );
        }

        /// <summary>
        /// Reads an array of signed bytes, whose length is a leading 32-bit integer.
        /// </summary>
        public sbyte[] ReadBinary()
        {
            int length = ReadInt32();
            // The array must be converted, not just casted, otherwise weird stuff happens when it's used
            byte[] bytes = new byte[length];
            _transport.ReadBytes( bytes );
            sbyte[] sbytes = new sbyte[length];
            Buffer.BlockCopy( bytes, 0, sbytes, 0, length );
            return sbytes;
        }


        /// <summary>
        /// Writes the specified message header.
        /// </summary>
        public void WriteMessageHeader( ThriftMessageHeader header )
        {
            WriteInt32( (int) ( Version1 | (uint) header.MessageType ) );
            WriteString( header.Name );
            WriteInt32( header.Id );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void WriteMessageEnd() { }

        /// <summary>
        /// Writes the specified struct header.
        /// </summary>
        public void WriteStructHeader( ThriftStructHeader header ) { }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void WriteStructEnd() { }

        /// <summary>
        /// Writes the specified field header.
        /// </summary>
        public void WriteFieldHeader( ThriftFieldHeader header )
        {
            _transport.WriteByte( (byte) header.FieldTypeId );
            WriteInt16( header.Id );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void WriteFieldEnd() { }

        /// <summary>
        /// Writes a token signaling the end of fields in a struct.
        /// </summary>
        public void WriteFieldStop()
        {
            _transport.WriteByte( ThriftFieldHeader.Stop );
        }

        /// <summary>
        /// Writes the specified list header.
        /// </summary>
        public void WriteListHeader( ThriftCollectionHeader header )
        {
            _transport.WriteByte( (byte) header.ElementTypeId );
            WriteInt32( header.Count );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void WriteListEnd() { }

        /// <summary>
        /// Writes the specified set header.
        /// </summary>
        public void WriteSetHeader( ThriftCollectionHeader header )
        {
            _transport.WriteByte( (byte) header.ElementTypeId );
            WriteInt32( header.Count );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void WriteSetEnd() { }

        /// <summary>
        /// Writes the specified map's header.
        /// </summary>
        public void WriteMapHeader( ThriftMapHeader header )
        {
            _transport.WriteByte( (byte) header.KeyTypeId );
            _transport.WriteByte( (byte) header.ValueTypeId );
            WriteInt32( header.Count );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void WriteMapEnd() { }

        /// <summary>
        /// Writes the specified boolean value as a byte, 1 for true and 0 for false.
        /// </summary>
        public void WriteBoolean( bool value )
        {
            _transport.WriteByte( value ? (byte) 1 : (byte) 0 );
        }

        /// <summary>
        /// Writes the specified signed byte.
        /// </summary>
        public void WriteSByte( sbyte value )
        {
            _transport.WriteByte( (byte) value );
        }

        /// <summary>
        /// Writes the specified double-precision floating-point number, using the IEEE 754 big-endian format.
        /// </summary>
        public void WriteDouble( double value )
        {
            WriteInt64( BitConverter.DoubleToInt64Bits( value ) );
        }

        /// <summary>
        /// Writes the specified 16-bit integer in big-endian format.
        /// </summary>
        public void WriteInt16( short value )
        {
            buffer2[0] = (byte) ( value >> 8 );
            buffer2[1] = (byte) value;
            _transport.WriteBytes( buffer2 );
        }

        /// <summary>
        /// Writes the specified 32-bit integer in big-endian format.
        /// </summary>
        public void WriteInt32( int value )
        {
            buffer4[0] = (byte) ( value >> 24 );
            buffer4[1] = (byte) ( value >> 16 );
            buffer4[2] = (byte) ( value >> 8 );
            buffer4[3] = (byte) value;
            _transport.WriteBytes( buffer4 );
        }

        /// <summary>
        /// Writes the specified 64-bit integer in big-endian format.
        /// </summary>
        public void WriteInt64( long value )
        {
            buffer8[0] = (byte) ( value >> 56 );
            buffer8[1] = (byte) ( value >> 48 );
            buffer8[2] = (byte) ( value >> 40 );
            buffer8[3] = (byte) ( value >> 32 );
            buffer8[4] = (byte) ( value >> 24 );
            buffer8[5] = (byte) ( value >> 16 );
            buffer8[6] = (byte) ( value >> 8 );
            buffer8[7] = (byte) value;
            _transport.WriteBytes( buffer8 );
        }

        /// <summary>
        /// Writes the specified string in UTF-8 encoding, leading it with its length as a 32-bit integer.
        /// </summary>
        public void WriteString( string value )
        {
            byte[] bytes = Encoding.UTF8.GetBytes( value );
            WriteInt32( bytes.Length );
            _transport.WriteBytes( bytes );
        }

        /// <summary>
        /// Writes the specified array of signed bytes, leading it with its length as a 32-bit integer.
        /// </summary>
        public void WriteBinary( sbyte[] value )
        {
            WriteInt32( value.Length );
            byte[] bytes = new byte[value.Length];
            Buffer.BlockCopy( value, 0, bytes, 0, value.Length );
            _transport.WriteBytes( bytes );
        }

        /// <summary>
        /// Asynchronously flushes the written data, and reads all input in advance.
        /// </summary>
        public Task FlushAndReadAsync()
        {
            return _transport.FlushAndReadAsync();
        }

        #region IDisposable implementation
        ~ThriftBinaryProtocol()
        {
            Dispose( false );
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private void Dispose( bool disposing )
        {
            _transport.Dispose();
        }
        #endregion
    }
}