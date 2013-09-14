using System;
using System.IO;
using System.Net;
using System.Threading;

namespace ThriftSharp.Transport
{
    /// <summary>
    /// Transports binary data over HTTP POST requests.
    /// </summary>
    internal sealed class ThriftHttpTransport : IThriftTransport, IDisposable
    {
        // The timeout for sending and receiving data, in milliseconds.
        private const int Timeout = 5000;

        private readonly string _url;
        private HttpWebRequest _request;
        private Stream _outputStream;
        private Stream _inputStream;


        /// <summary>
        /// Initializes a new instance of the BinaryHttpClientTransport class using the specified  URL.
        /// </summary>
        /// <param name="url">The URL, including the port if necessary.</param>
        public ThriftHttpTransport( string url )
        {
            _url = url;
        }


        /// <summary>
        /// Reads an unsigned byte.
        /// </summary>
        /// <returns>An unsighed byte.</returns>
        public byte ReadByte()
        {
            CheckRead();

            int retVal = _inputStream.ReadByte();
            if ( retVal == -1 )
            {
                throw new ThriftTransportException( "There are no bytes left to be read." );
            }
            return (byte) retVal;
        }

        /// <summary>
        /// Reads an array of unsigned bytes of the specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>An array of unsigned bytes.</returns>
        public byte[] ReadBytes( int length )
        {
            CheckRead();

            byte[] array = new byte[length];
            if ( _inputStream.Read( array, 0, array.Length ) != length )
            {
                throw new ThriftTransportException( "There are not enough bytes to be read." );
            }
            return array;
        }

        /// <summary>
        /// Writes the specified unsigned byte.
        /// </summary>
        /// <param name="b">The unsigned byte.</param>
        public void WriteByte( byte b )
        {
            CheckWrite();

            _outputStream.WriteByte( b );
        }

        /// <summary>
        /// Writes the specified array of unsigned bytes.
        /// </summary>
        /// <param name="bytes">The array of unsigned bytes.</param>
        public void WriteBytes( byte[] bytes )
        {
            CheckWrite();

            _outputStream.Write( bytes, 0, bytes.Length );
        }


        /// <summary>
        /// Ensures a read is possible.
        /// </summary>
        private void CheckRead()
        {
            if ( _outputStream != null )
            {
                Flush();
            }

            if ( _inputStream == null )
            {
                throw new ThriftTransportException( "Cannot read before writing." );
            }
        }

        /// <summary>
        /// Ensures a write is possible.
        /// </summary>
        private void CheckWrite()
        {
            if ( _outputStream == null )
            {
                Open();
            }

            if ( _inputStream != null )
            {
                throw new ThriftTransportException( "Cannot write while reading is in progress. Call Close() first." );
            }
        }


        /// <summary>
        /// Opens the transport.
        /// </summary>
        private void Open()
        {
            _request = (HttpWebRequest) WebRequest.Create( _url );
            _request.ContentType = "application/x-thrift";
            _request.Accept = "application/x-thrift";
            _request.Method = "POST";
            _outputStream = WaitOnBeginEnd( _request.BeginGetRequestStream, _request.EndGetRequestStream, Timeout );

            if ( _outputStream == null )
            {
                throw new ThriftTransportException( string.Format( "The timeout ({0} ms) to send a request was exceeded.", Timeout ) );
            }
        }

        /// <summary>
        /// Flushes the transport.
        /// </summary>
        private void Flush()
        {
            var resp = WaitOnBeginEnd( _request.BeginGetResponse, _request.EndGetResponse, Timeout );
            if ( resp == null )
            {
                throw new ThriftTransportException( string.Format( "The timeout ({0} ms) to get a response was exceeded.", Timeout ) );
            }

            _inputStream = resp.GetResponseStream();

            _outputStream.Dispose();
            _outputStream = null;
        }

        /// <summary>
        /// Utility method to synchronously wait on Begin/End methods, the old .NET async model.
        /// </summary>
        private static T WaitOnBeginEnd<T>( Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end, int timeout )
        {
            var evt = new AutoResetEvent( false );
            T result = default( T );
            begin( res => { result = end( res ); evt.Set(); }, null );
            evt.WaitOne( timeout );
            return result;
        }

        #region IDisposable implementation
        /// <summary>
        /// Finalizes this instance.
        /// </summary>
        ~ThriftHttpTransport()
        {
            Dispose( false );
        }

        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize( this );
            Dispose( true );
        }

        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        /// <param name="disposing">Whether the call comes from the Dispose method.</param>
        private void Dispose( bool disposing )
        {
            if ( _inputStream != null )
            {
                _inputStream.Dispose();
            }
            if ( _outputStream != null )
            {
                _outputStream.Dispose();
            }
        }
        #endregion
    }
}