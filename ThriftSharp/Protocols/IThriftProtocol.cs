// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Threading.Tasks;
using ThriftSharp.Internals;

namespace ThriftSharp.Protocols
{
    /// <summary>
    /// Represents a Thrift protocol that can send and receive primitive types as well as containers, structs and messages.
    /// </summary>
    internal interface IThriftProtocol : IDisposable
    {
        /// <summary>
        /// Asynchronously reads a message header.
        /// </summary>
        /// <returns>A message header.</returns>
        Task<ThriftMessageHeader> ReadMessageHeaderAsync();

        /// <summary>
        /// Asynchronously reads an end-of-message token.
        /// </summary>
        Task ReadMessageEndAsync();

        /// <summary>
        /// Asynchronously reads a struct header.
        /// </summary>
        /// <returns>A struct header.</returns>
        Task<ThriftStructHeader> ReadStructHeaderAsync();

        /// <summary>
        /// Asynchronously reads an end-of-struct token.
        /// </summary>
        Task ReadStructEndAsync();

        /// <summary>
        /// Asynchronously reads a field header.
        /// </summary>
        /// <returns>
        /// A field header, or null if an end-of-field token was encountered.
        /// </returns>
        Task<ThriftFieldHeader> ReadFieldHeaderAsync();

        /// <summary>
        /// Asynchronously reads an end-of-field token.
        /// </summary>
        Task ReadFieldEndAsync();

        /// <summary>
        /// Asynchronously reads a list header.
        /// </summary>
        /// <returns>A list header.</returns>
        Task<ThriftCollectionHeader> ReadListHeaderAsync();

        /// <summary>
        /// Asynchronously reads an end-of-list token.
        /// </summary>
        Task ReadListEndAsync();

        /// <summary>
        /// Asynchronously reads a set header.
        /// </summary>
        /// <returns>The set header.</returns>
        Task<ThriftCollectionHeader> ReadSetHeaderAsync();

        /// <summary>
        /// Asynchronously reads an end-of-set token.
        /// </summary>
        Task ReadSetEndAsync();

        /// <summary>
        /// Asynchronously reads a map header.
        /// </summary>
        /// <returns>A map header.</returns>
        Task<ThriftMapHeader> ReadMapHeaderAsync();

        /// <summary>
        /// Asynchronously reads an end-of-map token.
        /// </summary>
        Task ReadMapEndAsync();

        /// <summary>
        /// Asynchronously reads a boolean value.
        /// </summary>
        /// <returns>A boolean value.</returns>
        Task<bool> ReadBooleanAsync();

        /// <summary>
        /// Asynchronously reads a signed byte.
        /// </summary>
        /// <returns>A signed byte.</returns>
        Task<sbyte> ReadSByteAsync();

        /// <summary>
        /// Asynchronously reads an IEEE 754 double-precision floating-point number.
        /// </summary>
        /// <returns>An IEEE 754 double-precision floating-point number.</returns>
        Task<double> ReadDoubleAsync();

        /// <summary>
        /// Asynchronously reads a 16-bit integer.
        /// </summary>
        /// <returns>A 16-bit integer.</returns>
        Task<short> ReadInt16Async();

        /// <summary>
        /// Asynchronously reads a 32-bit integer.
        /// </summary>
        /// <returns>A 32-bit integer.</returns>
        Task<int> ReadInt32Async();

        /// <summary>
        /// Asynchronously reads a 64-bit integer.
        /// </summary>
        /// <returns>A 64-bit integer.</returns>
        Task<long> ReadInt64Async();

        /// <summary>
        /// Asynchronously reads a string.
        /// </summary>
        /// <returns>A string.</returns>
        Task<string> ReadStringAsync();

        /// <summary>
        /// Asynchronously reads an array of signed bytes.
        /// </summary>
        /// <returns>An array of signed bytes.</returns>
        Task<sbyte[]> ReadBinaryAsync();


        /// <summary>
        /// Writes the specified message header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteMessageHeader( ThriftMessageHeader header );

        /// <summary>
        /// Writes an end-of-message token.
        /// </summary>
        void WriteMessageEnd();

        /// <summary>
        /// Writes the specified struct header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteStructHeader( ThriftStructHeader header );

        /// <summary>
        /// Writes an end-of-struct token.
        /// </summary>
        void WriteStructEnd();

        /// <summary>
        /// Writes the specified field header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteFieldHeader( ThriftFieldHeader header );

        /// <summary>
        /// Writes an end-of-field token.
        /// </summary>
        void WriteFieldEnd();

        /// <summary>
        /// Writes a token signaling the end of fields in a struct.
        /// </summary>
        void WriteFieldStop();

        /// <summary>
        /// Writes the specified list header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteListHeader( ThriftCollectionHeader header );

        /// <summary>
        /// Writes an end-of-list token.
        /// </summary>
        void WriteListEnd();

        /// <summary>
        /// Writes the specified set header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteSetHeader( ThriftCollectionHeader header );

        /// <summary>
        /// Writes an end-of-set token.
        /// </summary>
        void WriteSetEnd();

        /// <summary>
        /// Writes the specified map header.
        /// </summary>
        /// <param name="header">The header.</param>
        void WriteMapHeader( ThriftMapHeader header );

        /// <summary>
        /// Writes an end-of-map token.
        /// </summary>
        void WriteMapEnd();

        /// <summary>
        /// Writes the specified boolean value.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        void WriteBoolean( bool value );

        /// <summary>
        /// Writes the specified signed byte.
        /// </summary>
        /// <param name="value">The signed byte value.</param>
        void WriteSByte( sbyte value );

        /// <summary>
        /// Writes the specified IEEE 754 double-precision floating-point number.
        /// </summary>
        /// <param name="value">The floating point value.</param>
        void WriteDouble( double value );

        /// <summary>
        /// Writes the specified 16-bit signed integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        void WriteInt16( short value );

        /// <summary>
        /// Writes the specified 32-bit signed integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        void WriteInt32( int value );

        /// <summary>
        /// Writes the specified 64-bit signed integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        void WriteInt64( long value );

        /// <summary>
        /// Writes the specified string.
        /// </summary>
        /// <param name="value">The string.</param>
        void WriteString( string value );

        /// <summary>
        /// Writes the specified array of signed bytes.
        /// </summary>
        /// <param name="value">The array of signed bytes.</param>
        void WriteBinary( sbyte[] value );

        /// <summary>
        /// Asynchronously flushes the written data.
        /// </summary>
        Task FlushAsync();
    }
}