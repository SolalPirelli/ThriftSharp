// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;

namespace ThriftSharp.Transport
{
    /// <summary>
    /// Transmits binary data at the byte level.
    /// </summary>
    internal interface IThriftTransport : IDisposable
    {
        /// <summary>
        /// Reads an unsigned byte.
        /// </summary>
        /// <returns>An unsighed byte.</returns>
        byte ReadByte();

        /// <summary>
        /// Reads an array of unsigned bytes of the specified length.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <returns>An array of unsigned bytes.</returns>
        byte[] ReadBytes( int length );


        /// <summary>
        /// Writes the specified unsigned byte.
        /// </summary>
        /// <param name="b">The unsigned byte.</param>
        void WriteByte( byte b );

        /// <summary>
        /// Writes the specified array of unsigned bytes.
        /// </summary>
        /// <param name="bytes">The array of unsigned bytes.</param>
        void WriteBytes( byte[] bytes );
    }
}