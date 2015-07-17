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

        // PERF: Cached buffers for writing i16, i32 and i64
        private readonly byte[] i16Buffer = new byte[2], i32Buffer = new byte[4], i64Buffer = new byte[8];

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
                byte[] nameBytes = _transport.ReadBytes( size );
                string name = Encoding.UTF8.GetString( nameBytes, 0, nameBytes.Length );
                var type = (ThriftMessageType) _transport.ReadByte();
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
            byte tid = _transport.ReadByte();
            if ( tid == ThriftFieldHeader.Stop )
            {
                return null;
            }

            short id = ReadInt16();
            return new ThriftFieldHeader( id, "", (ThriftTypeId) tid );
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
            byte tid = _transport.ReadByte();
            int count = ReadInt32();
            return new ThriftCollectionHeader( count, (ThriftTypeId) tid );
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
            byte tid = _transport.ReadByte();
            int count = ReadInt32();
            return new ThriftCollectionHeader( count, (ThriftTypeId) tid );
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
            var keyTypeId = _transport.ReadByte();
            var valueTypeId = _transport.ReadByte();
            int count = ReadInt32();
            return new ThriftMapHeader( count, (ThriftTypeId) keyTypeId, (ThriftTypeId) valueTypeId );
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
            return _transport.ReadByte() != 0;
        }

        /// <summary>
        /// Reads a signed byte.
        /// </summary>
        public sbyte ReadSByte()
        {
            return (sbyte) _transport.ReadByte();
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
            return BitConverter.ToInt16( ReadBigEndianBytes( 2 ), 0 );
        }

        /// <summary>
        /// Reads a big-endian 32-bit integer.
        /// </summary>
        public int ReadInt32()
        {
            return BitConverter.ToInt32( ReadBigEndianBytes( 4 ), 0 );
        }

        /// <summary>
        /// Reads a big-endian 64-bit integer.
        /// </summary>
        public long ReadInt64()
        {
            return BitConverter.ToInt64( ReadBigEndianBytes( 8 ), 0 );
        }

        /// <summary>
        /// Reads an UTF-8 string, whose length is a leading 32-bit integer.
        /// </summary>
        public string ReadString()
        {
            int length = ReadInt32();
            byte[] bytes = _transport.ReadBytes( length );
            return Encoding.UTF8.GetString( bytes, 0, bytes.Length );
        }

        /// <summary>
        /// Reads an array of signed bytes, whose length is a leading 32-bit integer.
        /// </summary>
        public sbyte[] ReadBinary()
        {
            int length = ReadInt32();
            // The array must be converted, not just casted, otherwise weird stuff happens when it's used
            byte[] bytes = _transport.ReadBytes( length );
            sbyte[] sbytes = new sbyte[length];
            for ( int n = 0; n < length; n++ )
            {
                sbytes[n] = (sbyte) bytes[n];
            }
            return sbytes;
        }

        /// <summary>
        /// Not part of the IThriftProtocol interface.
        /// Reads an array of unsigned bytes of the specified length representing a number, ensuring they are in big-endian order.
        /// </summary>
        private byte[] ReadBigEndianBytes( int length )
        {
            byte[] bytes = _transport.ReadBytes( length );
            if ( BitConverter.IsLittleEndian )
            {
                Array.Reverse( bytes );
            }
            return bytes;
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
            i16Buffer[0] = (byte) ( 0xff & ( value >> 8 ) );
            i16Buffer[1] = (byte) ( 0xff & value );
            _transport.WriteBytes( i16Buffer );
        }

        /// <summary>
        /// Writes the specified 32-bit integer in big-endian format.
        /// </summary>
        public void WriteInt32( int value )
        {
            i32Buffer[0] = (byte) ( 0xff & ( value >> 24 ) );
            i32Buffer[1] = (byte) ( 0xff & ( value >> 16 ) );
            i32Buffer[2] = (byte) ( 0xff & ( value >> 8 ) );
            i32Buffer[3] = (byte) ( 0xff & value );
            _transport.WriteBytes( i32Buffer );
        }

        /// <summary>
        /// Writes the specified 64-bit integer in big-endian format.
        /// </summary>
        public void WriteInt64( long value )
        {
            i64Buffer[0] = (byte) ( 0xff & ( value >> 56 ) );
            i64Buffer[1] = (byte) ( 0xff & ( value >> 48 ) );
            i64Buffer[2] = (byte) ( 0xff & ( value >> 40 ) );
            i64Buffer[3] = (byte) ( 0xff & ( value >> 32 ) );
            i64Buffer[4] = (byte) ( 0xff & ( value >> 24 ) );
            i64Buffer[5] = (byte) ( 0xff & ( value >> 16 ) );
            i64Buffer[6] = (byte) ( 0xff & ( value >> 8 ) );
            i64Buffer[7] = (byte) ( 0xff & value );
            _transport.WriteBytes( i64Buffer );
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
            _transport.WriteBytes( (byte[]) (Array) value );
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