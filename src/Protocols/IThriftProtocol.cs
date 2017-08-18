// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;
using System.Threading.Tasks;
using ThriftSharp.Models;

namespace ThriftSharp.Protocols
{
    /// <summary>
    /// Protocol that can send and receive Thrift types and messages.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface will never be passed null arguments.
    /// </remarks>
    public interface IThriftProtocol : IDisposable
    {
        /// <summary>
        /// Reads a message header.
        /// </summary>
        ThriftMessageHeader ReadMessageHeader();

        /// <summary>
        /// Reads an end-of-message token.
        /// </summary>
        void ReadMessageEnd();

        /// <summary>
        /// Reads a struct header.
        /// </summary>
        ThriftStructHeader ReadStructHeader();

        /// <summary>
        /// Reads an end-of-struct token.
        /// </summary>
        void ReadStructEnd();

        /// <summary>
        /// Reads a field header.
        /// Returns null if there are no more fields in the struct currently being read.
        /// </summary>
        ThriftFieldHeader ReadFieldHeader();

        /// <summary>
        /// Reads an end-of-field token.
        /// </summary>
        void ReadFieldEnd();

        /// <summary>
        /// Reads a list header.
        /// </summary>
        ThriftCollectionHeader ReadListHeader();

        /// <summary>
        /// Reads an end-of-list token.
        /// </summary>
        void ReadListEnd();

        /// <summary>
        /// Reads a set header.
        /// </summary>
        ThriftCollectionHeader ReadSetHeader();

        /// <summary>
        /// Reads an end-of-set token.
        /// </summary>
        void ReadSetEnd();

        /// <summary>
        /// Reads a map header.
        /// </summary>
        ThriftMapHeader ReadMapHeader();

        /// <summary>
        /// Reads an end-of-map token.
        /// </summary>
        void ReadMapEnd();

        /// <summary>
        /// Reads a boolean.
        /// </summary>
        bool ReadBoolean();

        /// <summary>
        /// Reads a signed byte.
        /// </summary>
        sbyte ReadSByte();

        /// <summary>
        /// Reads an IEEE 754 double-precision floating-point number.
        /// </summary>
        double ReadDouble();

        /// <summary>
        /// Reads a 16-bit integer.
        /// </summary>
        short ReadInt16();

        /// <summary>
        /// Reads a 32-bit integer.
        /// </summary>
        int ReadInt32();

        /// <summary>
        /// Reads a 64-bit integer.
        /// </summary>
        long ReadInt64();

        /// <summary>
        /// Reads an UTF-8 string.
        /// </summary>
        string ReadString();

        /// <summary>
        /// Reads an array of signed bytes.
        /// </summary>
        sbyte[] ReadBinary();


        /// <summary>
        /// Asynchronously flushes the written data and reads all input.
        /// </summary>
        Task FlushAndReadAsync();


        /// <summary>
        /// Writes the specified message header.
        /// </summary>
        void WriteMessageHeader( ThriftMessageHeader header );

        /// <summary>
        /// Writes an end-of-message token.
        /// </summary>
        void WriteMessageEnd();

        /// <summary>
        /// Writes the specified struct header.
        /// </summary>
        void WriteStructHeader( ThriftStructHeader header );

        /// <summary>
        /// Writes an end-of-struct token.
        /// </summary>
        void WriteStructEnd();

        /// <summary>
        /// Writes the specified field header.
        /// </summary>
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
        void WriteListHeader( ThriftCollectionHeader header );

        /// <summary>
        /// Writes an end-of-list token.
        /// </summary>
        void WriteListEnd();

        /// <summary>
        /// Writes the specified set header.
        /// </summary>
        void WriteSetHeader( ThriftCollectionHeader header );

        /// <summary>
        /// Writes an end-of-set token.
        /// </summary>
        void WriteSetEnd();

        /// <summary>
        /// Writes the specified map header.
        /// </summary>
        void WriteMapHeader( ThriftMapHeader header );

        /// <summary>
        /// Writes an end-of-map token.
        /// </summary>
        void WriteMapEnd();

        /// <summary>
        /// Writes the specified boolean.
        /// </summary>
        void WriteBoolean( bool value );

        /// <summary>
        /// Writes the specified signed byte.
        /// </summary>
        void WriteSByte( sbyte value );

        /// <summary>
        /// Writes the specified IEEE 754 double-precision floating-point number.
        /// </summary>
        void WriteDouble( double value );

        /// <summary>
        /// Writes the specified 16-bit signed integer.
        /// </summary>
        void WriteInt16( short value );

        /// <summary>
        /// Writes the specified 32-bit signed integer.
        /// </summary>
        void WriteInt32( int value );

        /// <summary>
        /// Writes the specified 64-bit signed integer.
        /// </summary>
        void WriteInt64( long value );

        /// <summary>
        /// Writes the specified string.
        /// </summary>
        void WriteString( string value );

        /// <summary>
        /// Writes the specified array of signed bytes.
        /// </summary>
        void WriteBinary( sbyte[] value );
    }
}