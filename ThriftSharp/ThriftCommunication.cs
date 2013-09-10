using System;
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
        /// <returns>A built ThriftCommunication object.</returns>
        ThriftCommunication OverHttp( string url );
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
        /// <returns>A built ThriftCommunication object.</returns>
        ThriftCommunication IThriftTransportPicker.OverHttp( string url )
        {
            return new ThriftCommunication( this, () => new ThriftHttpTransport( url ) );
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