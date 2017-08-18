// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using ThriftSharp.Protocols;
using ThriftSharp.Transport;
using ThriftSharp.Utilities;

namespace ThriftSharp
{
    /// <summary>
    /// Part of the build chain for Thrift communications.
    /// Picks the mode of transport.
    /// </summary>
    public interface IThriftTransportPicker : IFluent
    {
        /// <summary>
        /// Communicates over HTTP at the specified URL.
        /// </summary>
        /// <param name="url">The URL, including the port.</param>
        /// <param name="headers">Optional. The headers to use with the requests. No additional headers by default.</param>
        /// <param name="clientHandler">Optional. The HTTP handler. The default is the default HTTP handler.</param>
        /// <param name="timeout">Optional. The timeout in milliseconds. The default is 5 seconds.</param>
        /// <returns>A finished ThriftCommunication object.</returns>
        ThriftCommunication OverHttp( string url, IReadOnlyDictionary<string, string> headers = null,
                                      HttpMessageHandler clientHandler = null, TimeSpan? timeout = null );

        /// <summary>
        /// Communicates using the specified transport.
        /// </summary>
        /// <param name="transportCreator">A function taking a cancellation token and creating a transport from it.</param>
        /// <returns>A finished ThriftCommunication object.</returns>
        ThriftCommunication UsingCustomTransport( Func<CancellationToken, IThriftTransport> transportCreator );
    }

    /// <summary>
    /// Builds a Thrift communication method.
    /// </summary>
    public sealed class ThriftCommunication : IThriftTransportPicker
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds( 5 );

        private readonly Func<IThriftTransport, IThriftProtocol> _protocolCreator;
        private readonly Func<CancellationToken, IThriftTransport> _transportCreator;


        /// <summary>
        /// Initializes a new instance of the ThriftCommunication class with the specified protocol creator.
        /// </summary>
        private ThriftCommunication( Func<IThriftTransport, IThriftProtocol> protocolCreator )
        {
            _protocolCreator = protocolCreator;
        }

        /// <summary>
        /// Initializes a new instance of the ThriftCommunication class as a second part of the build step with the specified transport factory.
        /// </summary>
        private ThriftCommunication( ThriftCommunication comm, Func<CancellationToken, IThriftTransport> transportCreator )
        {
            _protocolCreator = comm._protocolCreator;
            _transportCreator = transportCreator;
        }


        /// <summary>
        /// Transmit data in binary format.
        /// </summary>
        /// <returns>A builder object to select the means of transport.</returns>
        public static IThriftTransportPicker Binary()
        {
            return new ThriftCommunication( t => new ThriftBinaryProtocol( t ) );
        }

        /// <summary>
        /// Communicates using the specified protocol.
        /// </summary>
        /// <param name="protocolCreator">A function taking a transport and creating a protocol from it.</param>
        /// <returns>A builder object to select the means of transport.</returns>
        public static IThriftTransportPicker UsingCustomProtocol( Func<IThriftTransport, IThriftProtocol> protocolCreator )
        {
            Validation.IsNotNull( protocolCreator, nameof( protocolCreator ) );

            return new ThriftCommunication( protocolCreator );
        }


        /// <summary>
        /// Communicate over HTTP at the specified URL.
        /// </summary>
        ThriftCommunication IThriftTransportPicker.OverHttp( string url, IReadOnlyDictionary<string, string> headers,
                                                             HttpMessageHandler clientHandler, TimeSpan? timeout )
        {
            Validation.IsNeitherNullNorWhitespace( url, nameof( url ) );

            var realHeaders = headers ?? new Dictionary<string, string>();
            var realHandler = clientHandler ?? new HttpClientHandler();
            var realTimeout = timeout ?? DefaultTimeout;

            return new ThriftCommunication( this, token => new ThriftHttpTransport( url, realHeaders, realHandler, token, realTimeout ) );
        }

        /// <summary>
        /// Communicates using the specified transport.
        /// </summary>
        ThriftCommunication IThriftTransportPicker.UsingCustomTransport( Func<CancellationToken, IThriftTransport> transportCreator )
        {
            Validation.IsNotNull( transportCreator, nameof( transportCreator ) );

            return new ThriftCommunication( this, transportCreator );
        }


        /// <summary>
        /// Creates a single-use IThriftProtocol object.
        /// </summary>
        internal IThriftProtocol CreateProtocol( CancellationToken token )
        {
            return _protocolCreator( _transportCreator( token ) );
        }

        #region Static object methods hiding
        /// <summary>
        /// Redeclaration hiding the <see cref="object.Equals(object, object)" /> method from IntelliSense.
        /// Do not use this method.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public static new object Equals( object objA, object objB )
        {
            throw new InvalidOperationException( "Do not call ThriftCommunication.Equals. Use Object.Equals instead." );
        }

        /// <summary>
        /// Redeclaration hiding the <see cref="object.ReferenceEquals" /> method from IntelliSense.
        /// Do not use this method.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public static new object ReferenceEquals( object objA, object objB )
        {
            throw new InvalidOperationException( "Do not call ThriftCommunication.ReferenceEquals. Use Object.ReferenceEquals instead." );
        }
        #endregion
    }
}