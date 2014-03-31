// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

namespace ThriftSharp.Internals
{
    /// <summary>
    /// The available Thrift message types.
    /// </summary>
    internal enum ThriftMessageType
    {
        /// <summary>
        /// Client to server. Indicates a method call.
        /// </summary>
        Call = 1,

        /// <summary>
        /// Server to client. Indicates a method reply.
        /// </summary>
        Reply = 2,

        /// <summary>
        /// Server to client. Indicates an exception occured during a method processing.
        /// </summary>
        /// <remarks>
        /// Exceptions sent using ThriftMessageType.Exception are fatal errors that occured during processing, not the declared "throws" exceptions.
        /// </remarks>
        Exception = 3,

        /// <summary>
        /// Client to server. Indicates a one-way method call, i.e. a call that does not expect a reply.
        /// </summary>
        OneWay = 4
    }
}