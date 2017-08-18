// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

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
        /// <param name="offset">The offset at which to start.</param>
        /// <param name="count">The number of bytes to write.</param>
        void WriteBytes( byte[] bytes, int offset, int count );

        /// <summary>
        /// Asynchronously flushes the written bytes and reads all input bytes.
        /// </summary>
        Task FlushAndReadAsync();

        /// <summary>
        /// Reads unsigned bytes, and puts them in the specified array.
        /// </summary>
        /// <param name="output">The array in which to write read bytes.</param>
        /// <param name="offset">The offset at which to start writing in the array.</param>
        /// <param name="count">The number of bytes to read.</param>
        void ReadBytes( byte[] output, int offset, int count );
    }
}