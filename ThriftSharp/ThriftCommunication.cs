// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        /// <param name="timeout">Optional. The timeout in milliseconds. The default is 5 seconds; use -1 for an infinite timeout.</param>
        /// <param name="headers">Optional. The headers to use with the requests. No additional headers by default.</param>    
        /// <returns>A finished ThriftCommunication object.</returns>
        ThriftCommunication OverHttp( string url, int timeout = 5000, IDictionary<string, string> headers = null );
    }

    /// <summary>
    /// Builds a Thrift communication method.
    /// </summary>
    public sealed class ThriftCommunication : IThriftTransportPicker, IFluent
    {
        private readonly Func<IThriftTransport, IThriftProtocol> _protocolCreator;
        private readonly Func<IThriftTransport> _transportFactory;

        /// <summary>
        /// Initializes a new instance of the ThriftCommunication class with the specified protocol creator.
        /// </summary>
        private ThriftCommunication( Func<IThriftTransport, IThriftProtocol> protocolCreator )
        {
            _protocolCreator = protocolCreator;
        }

        /// <summary>
        /// Initializes a new instance of the ThriftCommunication class 
        /// as a second part of the build step with the specified transport factory.
        /// </summary>
        private ThriftCommunication( ThriftCommunication comm, Func<IThriftTransport> transportFactory )
        {
            _protocolCreator = comm._protocolCreator;
            _transportFactory = transportFactory;
        }

        /// <summary>
        /// Initializes a new instance of the ThriftCommunication class
        /// from the specified protocol.
        /// </summary>
        /// <remarks>
        /// This should only be used in unit tests.
        /// </remarks>
        [Obsolete( "Only use this constructor in unit tests." )]
        internal ThriftCommunication( IThriftProtocol protocol )
        {
            _protocolCreator = _ => protocol;
            _transportFactory = () => null;
        }

        /// <summary>
        /// Transmit data in binary format.
        /// </summary>
        public static IThriftTransportPicker Binary()
        {
            return new ThriftCommunication( t => new ThriftBinaryProtocol( t ) );
        }

        /// <summary>
        /// Communicates over HTTP at the specified URL.
        /// </summary>
        /// <param name="url">The URL, including the port.</param>
        /// <param name="timeout">Optional. The timeout in milliseconds. The default is 5 seconds; use -1 for an infinite timeout.</param>
        /// <param name="headers">Optional. The headers to use with the requests. No additional headers by default.</param>
        /// <returns>A finished ThriftCommunication object.</returns>
        ThriftCommunication IThriftTransportPicker.OverHttp( string url, int timeout, IDictionary<string, string> headers )
        {
            Validation.IsNeitherNullNorWhitespace( url, () => url );

            return new ThriftCommunication( this, () => new ThriftHttpTransport( url, headers ?? new Dictionary<string, string>(), timeout ) );
        }


        /// <summary>
        /// Creates a single-use IThriftProtocol object.
        /// </summary>
        internal IThriftProtocol CreateProtocol()
        {
            return _protocolCreator( _transportFactory() );
        }

        #region Static object methods hiding
        /// <summary>
        /// Redeclaration that hides the <see cref="object.Equals(object, object)" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public static new object Equals( object objA, object objB )
        {
            throw new InvalidOperationException( "Do not call ThriftCommunication.Equals. Use Object.Equals instead." );
        }

        /// <summary>
        /// Redeclaration that hides the <see cref="object.ReferenceEquals" /> method from IntelliSense.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public static new object ReferenceEquals( object objA, object objB )
        {
            throw new InvalidOperationException( "Do not call ThriftCommunication.ReferenceEquals. Use Object.ReferenceEquals instead." );
        }
        #endregion
    }
}