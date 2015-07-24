// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

namespace ThriftSharp
{
    /// <summary>
    /// The Thrift protocol exception types.
    /// </summary>
    [ThriftEnum]
    public enum ThriftProtocolExceptionType
    {
        /// <summary>
        /// An unknown exception occured.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The method requested by the client is unknown to the server.
        /// </summary>
        UnknownMethod = 1,

        /// <summary>
        /// The message type is invalid.
        /// </summary>
        InvalidMessageType = 2,

        /// <summary>
        /// The method name sent by the client is unknown to the server.
        /// </summary>
        WrongMethodName = 3,

        /// <summary>
        /// The sequence ID of the message is wrong.
        /// </summary>
        BadSequenceId = 4,

        /// <summary>
        /// The server did not send a result when one was expected.
        /// </summary>
        MissingResult = 5,

        /// <summary>
        /// An internal error occured.
        /// </summary>
        InternalError = 6,

        /// <summary>
        /// A protocol error occured.
        /// This can happen when the client and server Thrift versions do not match.
        /// </summary>
        ProtocolError = 7,

        /// <summary>
        /// An invalid transform occured.
        /// </summary>
        InvalidTransform = 8,

        /// <summary>
        /// An invalid protocol was used.
        /// </summary>
        InvalidProtocol = 9,

        /// <summary>
        /// The client is of an unsupported type.
        /// </summary>
        UnsupportedClientType = 10
    }
}