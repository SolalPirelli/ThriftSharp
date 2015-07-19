// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.IO;
using System.Threading.Tasks;
using ThriftSharp.Transport;

namespace ThriftSharp.Benchmarking
{
    /// <summary>
    /// Simple looping transport for Thrift# that reads back from what was written.
    /// </summary>
    public sealed class LoopTransport : IThriftTransport
    {
        private MemoryStream _memory = new MemoryStream();

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

        public void PrepareRead()
        {
            _memory.Seek( 0, SeekOrigin.Begin );
        }

        public void Reset()
        {
            _memory = new MemoryStream();
        }

        public void Dispose()
        {
            _memory.Dispose();
        }
    }
}