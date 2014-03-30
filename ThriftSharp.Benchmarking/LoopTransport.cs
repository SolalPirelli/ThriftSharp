// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System.IO;
using ThriftSharp.Transport;

namespace ThriftSharp.Benchmarking
{
    /// <summary>
    /// Simple looping transport that will read back from what was written.
    /// </summary>
    public sealed class LoopTransport : IThriftTransport
    {
        private MemoryStream _memory;
        private bool _isReading = true;


        public byte ReadByte()
        {
            CheckRead();
            return (byte) _memory.ReadByte();
        }

        public byte[] ReadBytes( int length )
        {
            CheckRead();
            byte[] buffer = new byte[length];
            _memory.Read( buffer, 0, length );
            return buffer;
        }

        public void WriteByte( byte b )
        {
            CheckWrite();
            _memory.WriteByte( b );
        }

        public void WriteBytes( byte[] bytes )
        {
            CheckWrite();
            _memory.Write( bytes, 0, bytes.Length );
        }

        public void Dispose() { }

        private void CheckWrite()
        {
            if ( _isReading )
            {
                _isReading = false;
                _memory = new MemoryStream();
            }
        }

        private void CheckRead()
        {
            if ( !_isReading )
            {
                _isReading = true;
                _memory.Seek( 0, SeekOrigin.Begin );
            }
        }
    }
}