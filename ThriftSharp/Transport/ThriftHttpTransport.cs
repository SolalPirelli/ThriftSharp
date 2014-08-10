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
        private byte[] _receivedBytes;
        private int _receivedBytesIndex;


        /// <summary>
        /// Initializes a new instance of the ThriftHttpClientTransport class using the specified  URL.
        /// </summary>
        /// <param name="url">The URL, including the port if necessary.</param>
        /// <param name="token">The cancellation token that will cancel asynchronous tasks.</param>
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
        /// Reads an unsigned byte.
        /// </summary>
        /// <returns>An unsigned byte.</returns>
        public byte ReadByte()
        {
            if ( _receivedBytesIndex == _receivedBytes.Length )
            {
                throw new InvalidOperationException( "There are no bytes left to be read." );
            }
            return _receivedBytes[_receivedBytesIndex++];
        }

        /// <summary>
        /// Reads an array of unsigned bytes of the specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>An array of unsigned bytes.</returns>
        public byte[] ReadBytes( int length )
        {
            if ( _receivedBytesIndex + length >= _receivedBytes.Length )
            {
                throw new InvalidOperationException( "There are not enough bytes left to be read." );
            }

            byte[] buffer = new byte[length];
            Array.Copy( _receivedBytes, _receivedBytesIndex, buffer, 0, length );
            _receivedBytesIndex += length;
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
        /// Asynchronously flushes the written bytes, and reads all input bytes in advance.
        /// </summary>
        public async Task FlushAndReadAsync()
        {
            _token.ThrowIfCancellationRequested();

            var request = WebRequest.CreateHttp( _url );
            request.ContentType = request.Accept = ThriftContentType;
            request.Method = ThriftHttpMethod;

            foreach ( var header in _headers )
            {
                request.Headers[header.Key] = header.Value;
            }

            using ( var requestStream = await TaskEx.FromAsync( request.BeginGetRequestStream, request.EndGetRequestStream, _timeout, _token ) )
            {
                _outputStream.WriteTo( requestStream );
                requestStream.Flush();
            }

            // This call *must* appear before the GetResponse call
            // Silverlight and WP8 throw NotSupportedException otherwise.
            _outputStream.Dispose();

            var response = await TaskEx.FromAsync( request.BeginGetResponse, request.EndGetResponse, _timeout, _token );

            using ( var responseStream = new MemoryStream() )
            {
                await response.GetResponseStream().CopyToAsync( responseStream );
                _receivedBytes = responseStream.ToArray();
                _receivedBytesIndex = 0;
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
            if ( _outputStream != null )
            {
                _outputStream.Dispose();
            }
        }
        #endregion
    }
}