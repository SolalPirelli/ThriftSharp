// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

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
        public new string Message { get; internal set; }

        /// <summary>
        /// Gets the exception's type.
        /// </summary>
        [ThriftField( 2, false, "type" )]
        public ThriftProtocolExceptionType? ExceptionType { get; internal set; }

        /// <summary>
        /// For deserialization purposes only.
        /// </summary>
        private ThriftProtocolException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThriftProtocolException" /> class with the specified values.
        /// </summary>
        /// <param name="exceptionType">The exception type.</param>
        internal ThriftProtocolException( ThriftProtocolExceptionType exceptionType )
        {
            ExceptionType = exceptionType;
        }
    }
}