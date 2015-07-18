// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Threading.Tasks;

namespace ThriftSharp.Transport
{
    /// <summary>
    /// Transports binary data at the byte level.
    /// </summary>
    internal interface IThriftTransport : IDisposable
    {
        /// <summary>
        /// Writes the specified array of unsigned bytes.
        /// </summary>
        /// <param name="bytes">The array of unsigned bytes.</param>
        void WriteBytes( byte[] bytes );

        /// <summary>
        /// Asynchronously flushes the written bytes, and reads all input bytes in advance.
        /// </summary>
        Task FlushAndReadAsync();

        /// <summary>
        /// Reads unsigned bytes, and puts them in the specified array.
        /// </summary>
        /// <param name="output">The array in which to read bytes. It will be overwritten completely.</param>
        void ReadBytes( byte[] output );
    }
}