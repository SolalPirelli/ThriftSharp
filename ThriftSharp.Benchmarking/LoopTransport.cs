// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.IO;
using System.Threading.Tasks;
using ThriftSharp.Transport;

namespace ThriftSharp.Benchmarking
{
    /// <summary>
    /// Simple looping transport for Thrift# that will read back from what was written.
    /// </summary>
    public sealed class LoopTransport : IThriftTransport
    {
        private MemoryStream _memory = new MemoryStream();

        public byte ReadByte()
        {
            return (byte) _memory.ReadByte();
        }

        public byte[] ReadBytes( int length )
        {
            byte[] buffer = new byte[length];
            _memory.Read( buffer, 0, length );
            return buffer;
        }

        public void WriteByte( byte b )
        {
            _memory.WriteByte( b );
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