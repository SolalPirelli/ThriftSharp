// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

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

        private static readonly Task CompletedTask = Task.FromResult( 0 );

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
        /// Asynchronously reads a message header.
        /// </summary>
        /// <returns>A message header.</returns>
        public async Task<ThriftMessageHeader> ReadMessageHeaderAsync()
        {
            int size = await ReadInt32Async();
            if ( size < 0 )
            {
                uint version = (uint) size & VersionMask;
                if ( version != Version1 )
                {
                    throw new ThriftProtocolException( ThriftProtocolExceptionType.InvalidProtocol, "Invalid protocol version: " + version );
                }

                var type = (ThriftMessageType) ( size & 0xFF );
                string name = await ReadStringAsync();
                int id = await ReadInt32Async();
                return new ThriftMessageHeader( id, name, type );
            }
            else
            {
                // Old protocol version
                byte[] nameBytes = await _transport.ReadBytesAsync( size );
                string name = Encoding.UTF8.GetString( nameBytes, 0, nameBytes.Length );
                var type = (ThriftMessageType) await _transport.ReadByteAsync();
                int id = await ReadInt32Async();
                return new ThriftMessageHeader( id, name, type );
            }
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public Task ReadMessageEndAsync()
        {
            return CompletedTask;
        }

        /// <summary>
        /// Returns an empty struct header.
        /// The binary protocol does not store struct headers.
        /// </summary>
        /// <returns>An empty struct header.</returns>
        public Task<ThriftStructHeader> ReadStructHeaderAsync()
        {
            return Task.FromResult( new ThriftStructHeader( "" ) );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public Task ReadStructEndAsync()
        {
            return CompletedTask;
        }

        /// <summary>
        /// Asynchronously reads a field header.
        /// </summary>
        /// <returns>
        /// A field header, or null if an end-of-field token was encountered.
        /// </returns>
        public async Task<ThriftFieldHeader> ReadFieldHeaderAsync()
        {
            byte tid = await _transport.ReadByteAsync();
            if ( tid == ThriftFieldHeader.Stop )
            {
                return null;
            }

            short id = await ReadInt16Async();
            return new ThriftFieldHeader( id, "", (ThriftTypeId) tid );
        }

        /// <summary>
        /// Does nothing.
        /// The binary protocol does not use end tokens.
        /// </summary>
        public Task ReadFieldEndAsync()
        {
            return CompletedTask;
        }

        /// <summary>
        /// Asynchronously reads a list header.
        /// </summary>
        /// <returns>A list header.</returns>
        public async Task<ThriftCollectionHeader> ReadListHeaderAsync()
        {
            byte tid = await _transport.ReadByteAsync();
            int count = await ReadInt32Async();
            return new ThriftCollectionHeader( count, (ThriftTypeId) tid );
        }

        /// <summary>
        /// Does nothing. 
        /// The binary protocol does not use end tokens.
        /// </summary>
        public Task ReadListEndAsync()
        {
            return CompletedTask;
        }

        /// <summary>
        /// Asynchronously reads a set header.
        /// </summary>
        /// <returns>A set header.</returns>
        public async Task<ThriftCollectionHeader> ReadSetHeaderAsync()
        {
            byte tid = await _transport.ReadByteAsync();
            int count = await ReadInt32Async();
            return new ThriftCollectionHeader( count, (ThriftTypeId) tid );
        }

        /// <summary>
        /// Does nothing.
        /// The binary protocol does not use end tokens.
        /// </summary>
        public Task ReadSetEndAsync()
        {
            return CompletedTask;
        }

        /// <summary>
        /// Asynchronously reads a map header.
        /// </summary>
        /// <returns>A map header.</returns>
        public async Task<ThriftMapHeader> ReadMapHeaderAsync()
        {
            var keyTypeId = await _transport.ReadByteAsync();
            var valueTypeId = await _transport.ReadByteAsync();
            int count = await ReadInt32Async();
            return new ThriftMapHeader( count, (ThriftTypeId) keyTypeId, (ThriftTypeId) valueTypeId );
        }

        /// <summary>
        /// Does nothing.
        /// The binary protocol does not use end tokens.
        /// </summary>
        public Task ReadMapEndAsync()
        {
            return CompletedTask;
        }

        /// <summary>
        /// Asynchronously reads a boolean value as a byte, where 0 is true and anything else is false.
        /// </summary>
        /// <returns>A boolean.</returns>
        public async Task<bool> ReadBooleanAsync()
        {
            return await _transport.ReadByteAsync() != 0;
        }

        /// <summary>
        /// Asynchronously reads a signed byte.
        /// </summary>
        /// <returns>A signed byte.</returns>
        public async Task<sbyte> ReadSByteAsync()
        {
            return (sbyte) await _transport.ReadByteAsync();
        }

        /// <summary>
        /// Asynchronously reads a big-endian, double-precision floating-point number.
        /// </summary>
        /// <returns>A double-precision floating-point number.</returns>
        public async Task<double> ReadDoubleAsync()
        {
            return BitConverter.Int64BitsToDouble( await ReadInt64Async() );
        }

        /// <summary>
        /// Asynchronously reads a big-endian 16-bit integer.
        /// </summary>
        /// <returns>A 16-bit integer.</returns>
        public async Task<short> ReadInt16Async()
        {
            return BitConverter.ToInt16( await ReadBigEndianBytesAsync( 2 ), 0 );
        }

        /// <summary>
        /// Asynchronously reads a big-endian 32-bit integer.
        /// </summary>
        /// <returns>A 32-bit integer.</returns>
        public async Task<int> ReadInt32Async()
        {
            return BitConverter.ToInt32( await ReadBigEndianBytesAsync( 4 ), 0 );
        }

        /// <summary>
        /// Asynchronously reads a big-endian 64-bit integer.
        /// </summary>
        /// <returns>A 64-bit integer.</returns>
        public async Task<long> ReadInt64Async()
        {
            return BitConverter.ToInt64( await ReadBigEndianBytesAsync( 8 ), 0 );
        }

        /// <summary>
        /// Asynchronously reads an UTF-8 string, whose length is a leading 32-bit integer.
        /// </summary>
        /// <returns>A string.</returns>
        public async Task<string> ReadStringAsync()
        {
            int length = await ReadInt32Async();
            byte[] bytes = await _transport.ReadBytesAsync( length );
            return Encoding.UTF8.GetString( bytes, 0, bytes.Length );
        }

        /// <summary>
        /// Asynchronously reads an array of signed bytes, whose length is a leading 32-bit integer.
        /// </summary>
        /// <returns>An array of signed bytes.</returns>
        public async Task<sbyte[]> ReadBinaryAsync()
        {
            int length = await ReadInt32Async();
            // The array must be converted, not just casted, otherwise weird stuff happens when it's used
            byte[] bytes = await _transport.ReadBytesAsync( length );
            sbyte[] sbytes = new sbyte[length];
            for ( int n = 0; n < length; n++ )
            {
                sbytes[n] = (sbyte) bytes[n];
            }
            return sbytes;
        }

        /// <summary>
        /// Not part of the IThriftProtocol interface.
        /// Asynchronously reads an array of unsigned bytes of the specified length representing a number, ensuring they are in big-endian order.
        /// </summary>
        private async Task<byte[]> ReadBigEndianBytesAsync( int length )
        {
            byte[] bytes = await _transport.ReadBytesAsync( length );
            if ( BitConverter.IsLittleEndian )
            {
                Array.Reverse( bytes );
            }
            return bytes;
        }


        /// <summary>
        /// Writes the specified message header.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteMessageHeader( ThriftMessageHeader header )
        {
            WriteInt32( (int) ( Version1 | (uint) header.MessageType ) );
            WriteString( header.Name );
            WriteInt32( header.Id );
        }

        /// <summary>
        /// Does nothing. The binary protocol does not use end tokens.
        /// </summary>
        public void WriteMessageEnd() { }

        /// <summary>
        /// Writes the specified struct header.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteStructHeader( ThriftStructHeader header ) { }

        /// <summary>
        /// Does nothing. The binary protocol does not use end tokens.
        /// </summary>
        public void WriteStructEnd() { }

        /// <summary>
        /// Writes the specified field header.
        /// </summary>
        /// <param name="header">The header</param>
        public void WriteFieldHeader( ThriftFieldHeader header )
        {
            WriteByte( (byte) header.FieldTypeId );
            WriteInt16( header.Id );
        }

        /// <summary>
        /// Does nothing. The binary protocol does not use end tokens.
        /// </summary>
        public void WriteFieldEnd() { }

        /// <summary>
        /// Writes a token signaling the end of fields in a struct.
        /// </summary>
        public void WriteFieldStop()
        {
            WriteByte( ThriftFieldHeader.Stop );
        }

        /// <summary>
        /// Writes the specified list header.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteListHeader( ThriftCollectionHeader header )
        {
            WriteByte( (byte) header.ElementTypeId );
            WriteInt32( header.Count );
        }

        /// <summary>
        /// Does nothing. The binary protocol does not use end tokens.
        /// </summary>
        public void WriteListEnd() { }

        /// <summary>
        /// Writes the specified set header.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteSetHeader( ThriftCollectionHeader header )
        {
            WriteByte( (byte) header.ElementTypeId );
            WriteInt32( header.Count );
        }

        /// <summary>
        /// Does nothing. The binary protocol does not use end tokens.
        /// </summary>
        public void WriteSetEnd() { }

        /// <summary>
        /// Writes the specified map's header.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteMapHeader( ThriftMapHeader header )
        {
            WriteByte( (byte) header.KeyTypeId );
            WriteByte( (byte) header.ValueTypeId );
            WriteInt32( header.Count );
        }

        /// <summary>
        /// Does nothing. The binary protocol does not use end tokens.
        /// </summary>
        public void WriteMapEnd() { }

        /// <summary>
        /// Writes the specified boolean value, 0 for true and 1 for false.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        public void WriteBoolean( bool value )
        {
            WriteByte( value ? (byte) 1 : (byte) 0 );
        }

        /// <summary>
        /// Writes the specified signed byte.
        /// </summary>
        /// <param name="value">The signed byte.</param>
        public void WriteSByte( sbyte value )
        {
            WriteByte( (byte) value );
        }

        /// <summary>
        /// Writes the specified double-precision floating-point number, using the IEEE 754 big-endian format.
        /// </summary>
        /// <param name="value">The double-precision floating-point number.</param>
        public void WriteDouble( double value )
        {
            WriteInt64( BitConverter.DoubleToInt64Bits( value ) );
        }

        /// <summary>
        /// Writes the specified 16-bit integer in big-endian format.
        /// </summary>
        /// <param name="value">The 16-bit integer.</param>
        public void WriteInt16( short value )
        {
            WriteBigEndianBytes( BitConverter.GetBytes( value ) );
        }

        /// <summary>
        /// Writes the specified 32-bit integer in big-endian format.
        /// </summary>
        /// <param name="value">The 32-bit integer.</param>
        public void WriteInt32( int value )
        {
            WriteBigEndianBytes( BitConverter.GetBytes( value ) );
        }

        /// <summary>
        /// Writes the specified 64-bit integer in big-endian format.
        /// </summary>
        /// <param name="value">The 64-bit integer.</param>
        public void WriteInt64( long value )
        {
            WriteBigEndianBytes( BitConverter.GetBytes( value ) );
        }

        /// <summary>
        /// Writes the specified string in UTF-8 encoding, leading it with its length as a 32-bit integer.
        /// </summary>
        /// <param name="value">The string.</param>
        public void WriteString( string value )
        {
            byte[] bytes = Encoding.UTF8.GetBytes( value );
            WriteInt32( bytes.Length );
            _transport.WriteBytes( bytes );
        }

        /// <summary>
        /// Writes the specified array of signed bytes, leading it with its length as a 32-bit integer.
        /// </summary>
        /// <param name="bytes">The array of signed bytes.</param>
        public void WriteBinary( sbyte[] bytes )
        {
            WriteInt32( bytes.Length );
            _transport.WriteBytes( (byte[]) (Array) bytes );
        }

        /// <summary>
        /// Asynchronously flushes the written data.
        /// </summary>
        public Task FlushAsync()
        {
            return _transport.FlushAsync();
        }

        /// <summary>
        /// Not part of the IThriftProtocol interface.
        /// Writes the specified unsigned byte.
        /// </summary>
        private void WriteByte( byte b )
        {
            _transport.WriteByte( b );
        }

        /// <summary>
        /// Not part of the IThriftProtocol interface.
        /// Writes the specified array of unsigned bytes representing a number, ensuring it is in big-endian order.
        /// </summary>
        private void WriteBigEndianBytes( byte[] bytes )
        {
            if ( BitConverter.IsLittleEndian )
            {
                Array.Reverse( bytes );
            }
            _transport.WriteBytes( bytes );
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