// Copyright (c) 2014-16 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ThriftSharp.Transport
{
    /// <summary>
    /// Transports binary data over HTTP POST requests.
    /// </summary>
    public sealed class HttpThriftTransport : IThriftTransport
    {
        private readonly string _url;
        private readonly CancellationToken _token;
        private readonly IReadOnlyDictionary<string, string> _headers;
        private readonly HttpClient _client;

        private MemoryStream _outputStream;
        private Stream _inputStream;


        /// <summary>
        /// Initializes a new instance of the <see cref="HttpThriftTransport" /> class using the specified values.
        /// </summary>
        /// <param name="url">The URL, including the port if necessary.</param>
        /// <param name="token">The cancellation token that will cancel asynchronous tasks.</param>
        /// <param name="headers">The HTTP headers to include with every request.</param>
        /// <param name="timeout">The timeout.</param>
        public HttpThriftTransport( string url, CancellationToken token, IReadOnlyDictionary<string, string> headers, TimeSpan timeout )
        {
            _url = url;
            _token = token;
            _headers = headers;

            _client = new HttpClient { Timeout = timeout };

            _outputStream = new MemoryStream();
        }


        /// <summary>
        /// Writes the specified array of unsigned bytes.
        /// </summary>
        /// <param name="bytes">The array.</param>
        /// <param name="offset">The offset at which to start.</param>
        /// <param name="count">The number of bytes to write.</param>
        public void WriteBytes( byte[] bytes, int offset, int count )
        {
            _outputStream.Write( bytes, offset, count );
        }

        /// <summary>
        /// Reads unsigned bytes, and puts them in the specified array.
        /// </summary>
        /// <param name="output">The array in which to write read bytes.</param>
        /// <param name="offset">The offset at which to start writing in the array.</param>
        /// <param name="count">The number of bytes to read.</param>
        public void ReadBytes( byte[] output, int offset, int count )
        {
            _inputStream.Read( output, offset, count );
        }

        /// <summary>
        /// Asynchronously flushes the written bytes and reads all input.
        /// </summary>
        public async Task FlushAndReadAsync()
        {
            _token.ThrowIfCancellationRequested();

            _outputStream.Seek( 0, SeekOrigin.Begin );

            var request = new HttpRequestMessage( HttpMethod.Post, _url );
            request.Content = new StreamContent( _outputStream );
            request.Content.Headers.ContentType = new MediaTypeHeaderValue( "application/x-thrift" );
            request.Headers.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/x-thrift" ) );

            foreach( var header in _headers )
            {
                request.Headers.TryAddWithoutValidation( header.Key, header.Value );
            }

            var response = await _client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead, _token );

            _inputStream = await response.Content.ReadAsStreamAsync();
            _outputStream.Dispose();
            _outputStream = null;
        }

        #region IDisposable implementation
        /// <summary>
        /// Finalizes the <see cref="HttpThriftTransport" />.
        /// </summary>
        ~HttpThriftTransport()
        {
            DisposePrivate();
        }

        /// <summary>
        /// Disposes of the <see cref="HttpThriftTransport" />.
        /// </summary>
        public void Dispose()
        {
            DisposePrivate();
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Disposes of the <see cref="HttpThriftTransport" />'s internals.
        /// </summary>
        private void DisposePrivate()
        {
            if( _outputStream != null )
            {
                _outputStream.Dispose();
            }
            if( _inputStream != null )
            {
                _inputStream.Dispose();
            }
        }
        #endregion
    }
}