// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ThriftSharp.Transport
{
    /// <summary>
    /// Transports binary data over HTTP POST requests.
    /// </summary>
    internal sealed class ThriftHttpTransport : IThriftTransport, IDisposable
    {
        private readonly string _url;
        private readonly int _timeout;

        private HttpWebRequest _request;
        private Stream _outputStream;
        private Stream _inputStream;


        /// <summary>
        /// Initializes a new instance of the BinaryHttpClientTransport class using the specified  URL.
        /// </summary>
        /// <param name="url">The URL, including the port if necessary.</param>
        /// <param name="timeout">The timeout in milliseconds (or -1 for an infinite timeout).</param>
        public ThriftHttpTransport( string url, int timeout )
        {
            _url = url;
            _timeout = timeout;
        }


        /// <summary>
        /// Reads an unsigned byte.
        /// </summary>
        /// <returns>An unsigned byte.</returns>
        public byte ReadByte()
        {
            CheckRead();

            int retVal = _inputStream.ReadByte();
            if ( retVal == -1 )
            {
                throw new InvalidOperationException( "There are no bytes left to be read." );
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
                throw new InvalidOperationException( "There are not enough bytes to be read." );
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
                throw new InvalidOperationException( "Cannot read before writing." );
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
            _outputStream = WaitOnBeginEnd( _request.BeginGetRequestStream, _request.EndGetRequestStream, _timeout );

            if ( _outputStream == null )
            {
                throw new ThriftTransportException( string.Format( "The timeout ({0} ms) to send a request was exceeded.", _timeout ) );
            }
        }

        /// <summary>
        /// Flushes the transport.
        /// </summary>
        private void Flush()
        {
            // These two calls MUST appear before the GetResponse calls
            // Silverlight (and WP8) throws a NotSupportedException otherwise.
            _outputStream.Dispose();
            _outputStream = null;

            var response = WaitOnBeginEnd( _request.BeginGetResponse, _request.EndGetResponse, _timeout );
            if ( response == null )
            {
                throw new ThriftTransportException( string.Format( "The timeout ({0} ms) to get a response was exceeded.", _timeout ) );
            }

            _inputStream = response.GetResponseStream();
        }

        /// <summary>
        /// Utility method to synchronously wait on Begin/End methods, the old .NET async model.
        /// </summary>
        private static T WaitOnBeginEnd<T>( Func<AsyncCallback, object, IAsyncResult> begin, Func<IAsyncResult, T> end, int timeout )
        {
            var task = Task.Factory.FromAsync( begin, end, null );

            try
            {
                if ( task.Wait( timeout ) )
                {
                    return task.Result;
                }
                return default( T );
            }
            catch ( AggregateException )
            {
                // An exception was thrown during the execution of the Task.
                return default( T );
            }
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