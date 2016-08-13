// Copyright (c) 2014-16 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;
using System.Text;
using System.Threading.Tasks;
using ThriftSharp.Models;
using ThriftSharp.Transport;

namespace ThriftSharp.Protocols
{
    /// <summary>
    /// Thrift's binary protocol. 
    /// A compact but simple way to represent Thrift data.
    /// </summary>
    public sealed class ThriftBinaryProtocol : IThriftProtocol
    {
        // Current Thrift protocol version
        private const uint Version1 = 0x80010000;
        // Mask used to store more information in the message size field
        private const uint VersionMask = 0xffff0000;

        // PERF: Cached buffer to read and write fixed-length items
        private readonly byte[] _buffer =
        {
            0, 0, 0, 0, 0, 0, 0, 0, // 8 bytes for the i64, which is the longest
            1, // True
            0 // False / Field stop
        };
        private const int BufferOneIndex = 8, BufferZeroIndex = 9;

        private readonly IThriftTransport _transport;

        /// <summary>
        /// Initializes a new instance of the ThriftBinaryProtocol class using the specified transport.
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

            string name;
            ThriftMessageType type;
            if( size < 0 )
            {
                uint version = (uint) size & VersionMask;
                if( version != Version1 )
                {
                    throw new ThriftProtocolException( ThriftProtocolExceptionType.InvalidProtocol );
                }

                name = ReadString();
                type = (ThriftMessageType) ( size & 0xFF );
            }
            else
            {
                // Old protocol version
                byte[] nameBytes = new byte[size];
                _transport.ReadBytes( nameBytes, 0, size );
                name = Encoding.UTF8.GetString( nameBytes, 0, nameBytes.Length );

                _transport.ReadBytes( _buffer, 0, 1 );
                type = (ThriftMessageType) _buffer[0];
            }

            ReadInt32(); // Message sequence ID
            return new ThriftMessageHeader( name, type );
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
            return new ThriftStructHeader( null );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void ReadStructEnd() { }

        /// <summary>
        /// Reads a field header.
        /// Returns null if there are no more fields in the struct currently being read.
        /// </summary>
        public ThriftFieldHeader ReadFieldHeader()
        {
            _transport.ReadBytes( _buffer, 0, 1 );
            var tid = (ThriftTypeId) _buffer[0];
            if( tid == ThriftTypeId.Empty )
            {
                return new ThriftFieldHeader( 0, null, ThriftTypeId.Empty );
            }

            short id = ReadInt16();
            return new ThriftFieldHeader( id, null, tid );
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
            _transport.ReadBytes( _buffer, 0, 5 );
            int count = ( _buffer[1] << 24 ) | ( _buffer[2] << 16 ) | ( _buffer[3] << 8 ) | _buffer[4];
            return new ThriftCollectionHeader( count, (ThriftTypeId) _buffer[0] );
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
            _transport.ReadBytes( _buffer, 0, 5 );
            int count = ( _buffer[1] << 24 ) | ( _buffer[2] << 16 ) | ( _buffer[3] << 8 ) | _buffer[4];
            return new ThriftCollectionHeader( count, (ThriftTypeId) _buffer[0] );
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
            _transport.ReadBytes( _buffer, 0, 6 );
            int count = ( _buffer[2] << 24 ) | ( _buffer[3] << 16 ) | ( _buffer[4] << 8 ) | _buffer[5];
            return new ThriftMapHeader( count, (ThriftTypeId) _buffer[0], (ThriftTypeId) _buffer[1] );
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
            _transport.ReadBytes( _buffer, 0, 1 );
            return _buffer[0] != 0;
        }

        /// <summary>
        /// Reads a signed byte.
        /// </summary>
        public sbyte ReadSByte()
        {
            _transport.ReadBytes( _buffer, 0, 1 );
            return (sbyte) _buffer[0];
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
            _transport.ReadBytes( _buffer, 0, 2 );
            return (short) ( ( _buffer[0] << 8 ) | ( _buffer[1] ) );
        }

        /// <summary>
        /// Reads a big-endian 32-bit integer.
        /// </summary>
        public int ReadInt32()
        {
            _transport.ReadBytes( _buffer, 0, 4 );
            return ( _buffer[0] << 24 ) |
                   ( _buffer[1] << 16 ) |
                   ( _buffer[2] << 8 ) |
                   _buffer[3];
        }

        /// <summary>
        /// Reads a big-endian 64-bit integer.
        /// </summary>
        public long ReadInt64()
        {
            _transport.ReadBytes( _buffer, 0, 8 );
            return ( (long) _buffer[0] << 56 ) |
                   ( (long) _buffer[1] << 48 ) |
                   ( (long) _buffer[2] << 40 ) |
                   ( (long) _buffer[3] << 32 ) |
                   ( (long) _buffer[4] << 24 ) |
                   ( (long) _buffer[5] << 16 ) |
                   ( (long) _buffer[6] << 8 ) |
                   (long) _buffer[7];
        }

        /// <summary>
        /// Reads an UTF-8 string whose length is a leading 32-bit integer.
        /// </summary>
        public string ReadString()
        {
            int length = ReadInt32();
            byte[] bytes = new byte[length];
            _transport.ReadBytes( bytes, 0, length );
            return Encoding.UTF8.GetString( bytes, 0, length );
        }

        /// <summary>
        /// Reads an array of signed bytes whose length is a leading 32-bit integer.
        /// </summary>
        public sbyte[] ReadBinary()
        {
            int length = ReadInt32();
            byte[] bytes = new byte[length];
            _transport.ReadBytes( bytes, 0, length );
            sbyte[] sbytes = new sbyte[length];
            Buffer.BlockCopy( bytes, 0, sbytes, 0, length );
            return sbytes;
        }


        /// <summary>
        /// Asynchronously flushes the written data and reads all input.
        /// </summary>
        public Task FlushAndReadAsync()
        {
            return _transport.FlushAndReadAsync();
        }


        /// <summary>
        /// Writes the specified message header.
        /// </summary>
        public void WriteMessageHeader( ThriftMessageHeader header )
        {
            WriteInt32( (int) ( Version1 | (uint) header.MessageType ) );
            WriteString( header.Name );
            WriteInt32( 0 ); // Message sequence ID
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
            _buffer[0] = (byte) header.TypeId;
            _buffer[1] = (byte) ( header.Id >> 8 );
            _buffer[2] = (byte) header.Id;
            _transport.WriteBytes( _buffer, 0, 3 );
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
            _transport.WriteBytes( _buffer, BufferZeroIndex, 1 );
        }

        /// <summary>
        /// Writes the specified list header.
        /// </summary>
        public void WriteListHeader( ThriftCollectionHeader header )
        {
            _buffer[0] = (byte) header.ElementTypeId;
            _buffer[1] = (byte) ( header.Count >> 24 );
            _buffer[2] = (byte) ( header.Count >> 16 );
            _buffer[3] = (byte) ( header.Count >> 8 );
            _buffer[4] = (byte) header.Count;
            _transport.WriteBytes( _buffer, 0, 5 );
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
            _buffer[0] = (byte) header.ElementTypeId;
            _buffer[1] = (byte) ( header.Count >> 24 );
            _buffer[2] = (byte) ( header.Count >> 16 );
            _buffer[3] = (byte) ( header.Count >> 8 );
            _buffer[4] = (byte) header.Count;
            _transport.WriteBytes( _buffer, 0, 5 );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void WriteSetEnd() { }

        /// <summary>
        /// Writes the specified map header.
        /// </summary>
        public void WriteMapHeader( ThriftMapHeader header )
        {
            _buffer[0] = (byte) header.KeyTypeId;
            _buffer[1] = (byte) header.ValueTypeId;
            _buffer[2] = (byte) ( header.Count >> 24 );
            _buffer[3] = (byte) ( header.Count >> 16 );
            _buffer[4] = (byte) ( header.Count >> 8 );
            _buffer[5] = (byte) header.Count;
            _transport.WriteBytes( _buffer, 0, 6 );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public void WriteMapEnd() { }

        /// <summary>
        /// Writes the specified boolean as a byte, 1 for true and 0 for false.
        /// </summary>
        public void WriteBoolean( bool value )
        {
            if( value )
            {
                _transport.WriteBytes( _buffer, BufferOneIndex, 1 );
            }
            else
            {
                _transport.WriteBytes( _buffer, BufferZeroIndex, 1 );
            }
        }

        /// <summary>
        /// Writes the specified signed byte.
        /// </summary>
        public void WriteSByte( sbyte value )
        {
            _buffer[0] = (byte) value;
            _transport.WriteBytes( _buffer, 0, 1 );
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
            _buffer[0] = (byte) ( value >> 8 );
            _buffer[1] = (byte) value;
            _transport.WriteBytes( _buffer, 0, 2 );
        }

        /// <summary>
        /// Writes the specified 32-bit integer in big-endian format.
        /// </summary>
        public void WriteInt32( int value )
        {
            _buffer[0] = (byte) ( value >> 24 );
            _buffer[1] = (byte) ( value >> 16 );
            _buffer[2] = (byte) ( value >> 8 );
            _buffer[3] = (byte) value;
            _transport.WriteBytes( _buffer, 0, 4 );
        }

        /// <summary>
        /// Writes the specified 64-bit integer in big-endian format.
        /// </summary>
        public void WriteInt64( long value )
        {
            _buffer[0] = (byte) ( value >> 56 );
            _buffer[1] = (byte) ( value >> 48 );
            _buffer[2] = (byte) ( value >> 40 );
            _buffer[3] = (byte) ( value >> 32 );
            _buffer[4] = (byte) ( value >> 24 );
            _buffer[5] = (byte) ( value >> 16 );
            _buffer[6] = (byte) ( value >> 8 );
            _buffer[7] = (byte) value;
            _transport.WriteBytes( _buffer, 0, 8 );
        }

        /// <summary>
        /// Writes the specified string in UTF-8 encoding, leading it with its length as a 32-bit integer.
        /// </summary>
        public void WriteString( string value )
        {
            byte[] bytes = Encoding.UTF8.GetBytes( value );
            WriteInt32( bytes.Length );
            _transport.WriteBytes( bytes, 0, bytes.Length );
        }

        /// <summary>
        /// Writes the specified array of signed bytes, leading it with its length as a 32-bit integer.
        /// </summary>
        public void WriteBinary( sbyte[] value )
        {
            byte[] bytes = new byte[value.Length + 4];
            bytes[0] = (byte) ( value.Length >> 24 );
            bytes[1] = (byte) ( value.Length >> 16 );
            bytes[2] = (byte) ( value.Length >> 8 );
            bytes[3] = (byte) value.Length;
            Buffer.BlockCopy( value, 0, bytes, 4, value.Length );
            _transport.WriteBytes( bytes, 0, bytes.Length );
        }

        #region IDisposable implementation
        /// <summary>
        /// Finalizes the ThriftBinaryProtocol.
        /// </summary>
        ~ThriftBinaryProtocol()
        {
            DisposePrivate();
        }

        /// <summary>
        /// Disposes of the ThriftBinaryProtocol.
        /// </summary>
        public void Dispose()
        {
            DisposePrivate();
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Disposes of the ThriftBinaryProtocol's internals.
        /// </summary>
        private void DisposePrivate()
        {
            _transport.Dispose();
        }
        #endregion
    }
}