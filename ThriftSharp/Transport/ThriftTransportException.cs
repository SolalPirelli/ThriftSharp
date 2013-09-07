using System;

namespace ThriftSharp.Transport
{
    /// <summary>
    /// Occurs when a Thrift transport encounters a problem.
    /// </summary>
    public sealed class ThriftTransportException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ThriftTransportException class with the specified message.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public ThriftTransportException( string message ) : base( message ) { }
    }
}