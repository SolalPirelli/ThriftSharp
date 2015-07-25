// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;
using ThriftSharp.Transport;

namespace ThriftSharp.Benchmarking
{
    public sealed class MemoryBuffer : IThriftTransport
    {
        private MemoryStream _memory;


        public static byte[] Serialize<T>( T obj )
        {
            var thriftStruct = ThriftAttributesParser.ParseStruct( typeof( T ).GetTypeInfo() );
            var buffer = new MemoryBuffer() { _memory = new MemoryStream() };
            ThriftStructWriter.Write( thriftStruct, obj, new ThriftBinaryProtocol( buffer ) );
            return buffer._memory.GetBuffer();
        }

        public static T Deserialize<T>( byte[] bytes )
        {
            var thriftStruct = ThriftAttributesParser.ParseStruct( typeof( T ).GetTypeInfo() );
            var buffer = new MemoryBuffer { _memory = new MemoryStream( bytes ) };
            return ThriftStructReader.Read<T>( thriftStruct, new ThriftBinaryProtocol( buffer ) );
        }


        public void ReadBytes( byte[] output )
        {
            _memory.Read( output, 0, output.Length );
        }

        public void WriteBytes( byte[] bytes )
        {
            _memory.Write( bytes, 0, bytes.Length );
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