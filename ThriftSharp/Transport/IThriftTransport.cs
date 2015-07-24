// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Threading.Tasks;

namespace ThriftSharp.Transport
{
    /// <summary>
    /// Transports data at the byte level.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface will never be passed null arguments.
    /// </remarks>
    public interface IThriftTransport : IDisposable
    {
        /// <summary>
        /// Writes the specified array of unsigned bytes.
        /// </summary>
        /// <param name="bytes">The array.</param>
        void WriteBytes( byte[] bytes );

        /// <summary>
        /// Asynchronously flushes the written bytes and reads all input bytes.
        /// </summary>
        Task FlushAndReadAsync();

        /// <summary>
        /// Reads unsigned bytes, and puts them in the specified array.
        /// </summary>
        /// <param name="output">The array in which to read bytes. It will be overwritten completely.</param>
        void ReadBytes( byte[] output );
    }
}