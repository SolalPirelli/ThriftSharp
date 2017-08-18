// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;
using System.IO;
using System.Threading.Tasks;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;
using ThriftSharp.Transport;

namespace ThriftSharp.Benchmarking
{
    public sealed class MemoryBuffer : IThriftTransport
    {
        private MemoryStream _memory;


        public static ArraySegment<byte> Serialize<T>( T obj )
        {
            var buffer = new MemoryBuffer() { _memory = new MemoryStream() };
            ThriftStructWriter.Write( obj, new ThriftBinaryProtocol( buffer ) );

            return new ArraySegment<byte>( buffer._memory.GetBuffer() );
        }

        public static T Deserialize<T>( ArraySegment<byte> bytes )
        {
            var buffer = new MemoryBuffer { _memory = new MemoryStream( bytes.Array, bytes.Offset, bytes.Count ) };
            return ThriftStructReader.Read<T>( new ThriftBinaryProtocol( buffer ) );
        }


        public void ReadBytes( byte[] output, int offset, int count )
        {
            _memory.Read( output, offset, count );
        }

        public void WriteBytes( byte[] bytes, int offset, int count )
        {
            _memory.Write( bytes, offset, count );
        }

        public Task FlushAndReadAsync()
        {
            throw new Exception( "Don't use this." );
        }

        public void Dispose()
        {
            _memory.Dispose();
        }
    }
}