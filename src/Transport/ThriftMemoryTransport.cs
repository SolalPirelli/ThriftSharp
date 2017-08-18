using System;
using System.IO;
using System.Threading.Tasks;

namespace ThriftSharp.Transport
{
    /// <summary>
    /// In-memory Thrift transport for testing and custom serialization purposes.
    /// </summary>
    public sealed class ThriftMemoryTransport : IThriftTransport
    {
        private bool _hasFlushed;
        private bool _isDisposed;
        private MemoryStream _stream;


        /// <summary>
        /// Initializes a new instance of the <see cref="ThriftMemoryTransport" /> class.
        /// </summary>
        public ThriftMemoryTransport()
        {
            _stream = new MemoryStream();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThriftMemoryTransport" /> class using the specified internal buffer.
        /// The returned transport will behave as if the buffer contents had been written into it, and it had been flushed afterwards.
        /// </summary>
        /// <param name="internalBuffer">The buffer.</param>
        public ThriftMemoryTransport( byte[] internalBuffer )
        {
            _stream = new MemoryStream( internalBuffer );
            _hasFlushed = true;
        }

        /// <summary>
        /// Gets the internal buffer used by this transport.
        /// This method completely breaks the encapsulation offered by this class, and only exists for performance reasons.
        /// Do not use this method unless you know what you are doing.
        /// </summary>
        public byte[] GetInternalBuffer()
        {
            if( _isDisposed )
            {
                throw new ObjectDisposedException( nameof( ThriftMemoryTransport ) );
            }

            return _stream.GetBuffer();
        }

        /// <summary>
        /// Writes the specified array of unsigned bytes.
        /// </summary>
        /// <param name="bytes">The array.</param>
        /// <param name="offset">The offset at which to start.</param>
        /// <param name="count">The number of bytes to write.</param>
        public void WriteBytes( byte[] bytes, int offset, int count )
        {
            if( _isDisposed )
            {
                throw new ObjectDisposedException( nameof( ThriftMemoryTransport ) );
            }
            if( _hasFlushed )
            {
                throw new InvalidOperationException( "Cannot write after flushing." );
            }

            _stream.Write( bytes, offset, count );
        }

        /// <summary>
        /// Synchronously pretends to flush the data. The data that will be read after this call is the same that was written.
        /// </summary>
        public Task FlushAndReadAsync()
        {
            if( _isDisposed )
            {
                throw new ObjectDisposedException( nameof( ThriftMemoryTransport ) );
            }
            if( _hasFlushed )
            {
                throw new InvalidOperationException( "Already flushed." );
            }

            _stream.Seek( 0, SeekOrigin.Begin );
            _hasFlushed = true;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Reads unsigned bytes, and puts them in the specified array.
        /// </summary>
        /// <param name="output">The array in which to write read bytes.</param>
        /// <param name="offset">The offset at which to start writing in the array.</param>
        /// <param name="count">The number of bytes to read.</param>
        public void ReadBytes( byte[] output, int offset, int count )
        {
            if( _isDisposed )
            {
                throw new ObjectDisposedException( nameof( ThriftMemoryTransport ) );
            }
            if( !_hasFlushed )
            {
                throw new InvalidOperationException( "Cannot read before flushing." );
            }

            _stream.Read( output, offset, count );
        }

        /// <summary>
        /// Disposes of the <see cref="ThriftMemoryTransport" />, releasing all memory.
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;
            _stream = null;
        }
    }
}