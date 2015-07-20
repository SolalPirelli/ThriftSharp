// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

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
    internal sealed class HttpThriftTransport : IThriftTransport
    {
        private const string ThriftContentType = "application/x-thrift";
        private const string ThriftHttpMethod = "POST";

        private readonly string _url;
        private readonly CancellationToken _token;
        private readonly IReadOnlyDictionary<string, string> _headers;
        private readonly TimeSpan _timeout;

        private MemoryStream _outputStream;
        private MemoryStream _inputStream;


        /// <summary>
        /// Initializes a new instance of the ThriftHttpClientTransport class using the specified  URL.
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
            _timeout = timeout;

            _outputStream = new MemoryStream();
        }


        /// <summary>
        /// Reads unsigned bytes, and puts them in the specified array.
        /// </summary>
        /// <param name="output">The array in which to read bytes. It will be overwritten completely.</param>
        public void ReadBytes( byte[] output )
        {
            _inputStream.Read( output, 0, output.Length );
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

            using ( var requestStream = await request.GetRequestStreamAsync().TimeoutAfter( _timeout ) )
            {
                _outputStream.WriteTo( requestStream );
                _outputStream.Dispose();

                await requestStream.FlushAsync( _token );
            }

            _token.ThrowIfCancellationRequested();

            // Don't keep the output stream for longer than what's needed,
            // and don't create the input stream before it's needed either.
            _outputStream.Dispose();
            _outputStream = null;
            _inputStream = new MemoryStream();

            var response = await request.GetResponseAsync().TimeoutAfter( _timeout );
            await response.GetResponseStream().CopyToAsync( _inputStream );
        }

        #region IDisposable implementation
        /// <summary>
        /// Finalizes this instance.
        /// </summary>
        ~HttpThriftTransport()
        {
            Dispose( false );
        }

        /// <summary>
        /// Disposes of this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
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