﻿// Copyright (c) 2014-16 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

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
    public sealed class HttpThriftTransport : IThriftTransport
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
        /// Initializes a new instance of the HttpThriftTransport class using the specified values.
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

            var request = WebRequest.CreateHttp( _url );
            request.ContentType = request.Accept = ThriftContentType;
            request.Method = ThriftHttpMethod;

            foreach( var header in _headers )
            {
                request.Headers[header.Key] = header.Value;
            }

            // N.B.: GetRequestStreamAsync doesn't make any HTTP calls, only GetResponseAsync does.
            using( var requestStream = await request.GetRequestStreamAsync() )
            {
                _outputStream.WriteTo( requestStream );
                _outputStream.Dispose();
                requestStream.Flush();
            }

            // Don't keep the output stream for longer than what's needed,
            // and don't create the input stream before it's needed either.
            _outputStream.Dispose();
            _outputStream = null;
            _inputStream = new MemoryStream();

            var response = await request.GetResponseAsync().TimeoutAfter( _timeout );

            _token.ThrowIfCancellationRequested();

            await response.GetResponseStream().CopyToAsync( _inputStream );
            _inputStream.Seek( 0, SeekOrigin.Begin );

            _token.ThrowIfCancellationRequested();
        }

        #region IDisposable implementation
        /// <summary>
        /// Finalizes the HttpThriftTransport.
        /// </summary>
        ~HttpThriftTransport()
        {
            DisposePrivate();
        }

        /// <summary>
        /// Disposes of the HttpThriftTransport.
        /// </summary>
        public void Dispose()
        {
            DisposePrivate();
            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Disposes of the HttpThriftTransport's internals.
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