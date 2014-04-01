// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ThriftSharp.Utilities;

namespace ThriftSharp.Transport
{
    /// <summary>
    /// Transports binary data over HTTP POST requests.
    /// </summary>
    internal sealed class ThriftHttpTransport : IThriftTransport, IDisposable
    {
        private const string ThriftContentType = "application/x-thrift";
        private const string ThriftHttpMethod = "POST";

        private static readonly byte[] OneByteBuffer = new byte[1];

        private readonly string _url;
        private readonly CancellationToken _token;
        private readonly IDictionary<string, string> _headers;
        private readonly int _timeout;

        private readonly MemoryStream _outputStream;
        private Stream _inputStream;
        private HttpWebRequest _request;


        /// <summary>
        /// Initializes a new instance of the ThriftHttpClientTransport class using the specified  URL.
        /// </summary>
        /// <param name="url">The URL, including the port if necessary.</param>
        /// <param name="headers">The HTTP headers to include with every request.</param>
        /// <param name="timeout">The timeout in milliseconds (or -1 for an infinite timeout).</param>
        public ThriftHttpTransport( string url, CancellationToken token, IDictionary<string, string> headers, int timeout )
        {
            _url = url;
            _token = token;
            _headers = headers;
            _timeout = timeout;

            _outputStream = new MemoryStream();
        }


        /// <summary>
        /// Asynchronously eads an unsigned byte.
        /// </summary>
        /// <returns>An unsigned byte.</returns>
        public async Task<byte> ReadByteAsync()
        {
            if ( await _inputStream.ReadAsync( OneByteBuffer, 0, 1, _token ) != 1 )
            {
                throw new InvalidOperationException( "There are no bytes left to be read." );
            }
            return OneByteBuffer[0];
        }

        /// <summary>
        /// Asynchronously reads an array of unsigned bytes of the specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>An array of unsigned bytes.</returns>
        public async Task<byte[]> ReadBytesAsync( int length )
        {
            byte[] buffer = new byte[length];
            if ( await _inputStream.ReadAsync( buffer, 0, length, _token ) != length )
            {
                throw new InvalidOperationException( "There are not enough bytes to be read." );
            }
            return buffer;
        }

        /// <summary>
        /// Writes the specified unsigned byte.
        /// </summary>
        /// <param name="b">The unsigned byte.</param>
        public void WriteByte( byte b )
        {
            _outputStream.WriteByte( b );
        }

        /// <summary>
        /// Writes the specified array of unsigned bytes.
        /// </summary>
        /// <param name="bytes">The array of unsigned bytes.</param>
        public void WriteBytes( byte[] bytes )
        {
            _outputStream.Write( bytes, 0, bytes.Length );
        }


        /// <summary>
        /// Asynchronously flushes the written bytes.
        /// </summary>
        public async Task FlushAsync()
        {
            _token.ThrowIfCancellationRequested();

            _request = WebRequest.CreateHttp( _url );
            _request.ContentType = _request.Accept = ThriftContentType;
            _request.Method = ThriftHttpMethod;

            foreach ( var header in _headers )
            {
                _request.Headers[header.Key] = header.Value;
            }

            using ( var requestStream = await TaskEx.FromAsync( _request.BeginGetRequestStream, _request.EndGetRequestStream, _timeout ) )
            {
                _outputStream.WriteTo( requestStream );
                requestStream.Flush();
            }

            // This call MUST appear before the GetResponse call
            // Silverlight (and WP8) throws a NotSupportedException otherwise.
            _outputStream.Dispose();

            var response = await TaskEx.FromAsync( _request.BeginGetResponse, _request.EndGetResponse, _timeout );
            _inputStream = response.GetResponseStream();
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
            if ( _outputStream != null )
            {
                _outputStream.Dispose();
            }
            if ( _inputStream != null )
            {
                _inputStream.Dispose();
            }
        }
        #endregion
    }
}