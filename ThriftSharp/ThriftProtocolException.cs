using System;

namespace ThriftSharp
{
    /// <summary>
    /// Fatal server exception transmitted to the client instead of the message.
    /// </summary>
    [ThriftStruct( "TApplicationException" )]
    public sealed class ThriftProtocolException : Exception
    {
        /// <summary>
        /// Gets the exception's message.
        /// </summary>
        [ThriftField( 1, false, "message" )]
        public new string Message { get; private set; }

        /// <summary>
        /// Gets the exception's type.
        /// </summary>
        [ThriftField( 2, false, "type" )]
        public ThriftProtocolExceptionType ExceptionType { get; private set; }


        /// <summary>
        /// Initializes a new instance of the ThriftTransferException class.
        /// </summary>
        /// <remarks>
        /// For serialization purposes only.
        /// </remarks>
        public ThriftProtocolException() { }

        /// <summary>
        /// Initializes a new instance of the ThriftTransferException class with the specified values.
        /// </summary>
        /// <param name="exceptionType">The exception type.</param>
        /// <param name="message">The exception message.</param>
        public ThriftProtocolException( ThriftProtocolExceptionType exceptionType, string message )
        {
            Message = message;
            ExceptionType = exceptionType;
        }
    }
}