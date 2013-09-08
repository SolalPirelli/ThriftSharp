using System;
using ThriftSharp.Models;

// NOTE: Unfortunately, CallerMemberName is not available for .NET <4.5, which this PCL supports and logs the call.

namespace ThriftSharp.Protocols
{
    /// <summary>
    /// Wraps a Thrift protocol and logs each call to a configurable sink and logs the call.
    /// </summary>
    public sealed class ThriftLoggingProtocol : IThriftProtocol
    {
        private readonly IThriftProtocol _wrapped;
        private Action<string> _logger;


        /// <summary>
        /// Initializes a new instance of the ThriftLoggingProtocol class that wraps the specified protocol and logs calls using the specified sink and logs the call.
        /// </summary>
        /// <param name="wrapped">The protocol to be wrapped.</param>
        /// <param name="logger">The logging sink to use.</param>
        public ThriftLoggingProtocol( IThriftProtocol wrapped, Action<string> logger )
        {
            _wrapped = wrapped;
            _logger = logger;
        }


        /// <summary>
        /// Logs a call to the specified method name and logs the call.
        /// </summary>
        private void Log( string methodName )
        {
            _logger( methodName );
        }

        /// <summary>
        /// Logs a call to the specified method name, as well as the specified value and logs the call.
        /// </summary>
        private void Log( string methodName, object value )
        {
            _logger( methodName + " " + value );
        }


        /// <summary>
        /// Reads a message header and logs the call.
        /// </summary>
        /// <returns>A message header.</returns>
        public ThriftMessageHeader ReadMessageHeader()
        {
            var value = _wrapped.ReadMessageHeader();
            Log( "ReadMessageHeader", value );
            return value;
        }

        /// <summary>
        /// Reads an end-of-message token and logs the call.
        /// </summary>
        public void ReadMessageEnd()
        {
            _wrapped.ReadMessageEnd();
            Log( "ReadMessageEnd" );
        }

        /// <summary>
        /// Reads a struct header and logs the call.
        /// </summary>
        /// <returns>A struct header.</returns>
        public ThriftStructHeader ReadStructHeader()
        {
            var value = _wrapped.ReadStructHeader();
            Log( "ReadStructHeader", value );
            return value;
        }

        /// <summary>
        /// Reads an end-of-struct token and logs the call.
        /// </summary>
        public void ReadStructEnd()
        {
            _wrapped.ReadStructEnd();
            Log( "ReadStructEnd" );
        }

        /// <summary>
        /// Reads a field header and logs the call.
        /// </summary>
        /// <returns>
        /// A field header, or null if an end-of-field token was encountered.
        /// </returns>
        public ThriftFieldHeader ReadFieldHeader()
        {
            var value = _wrapped.ReadFieldHeader();
            if ( value == null )
            {
                Log( "ReadFieldHeader", "Stop" );
            }
            else
            {
                Log( "ReadFieldHeader", value );
            }
            return value;
        }

        /// <summary>
        /// Reads an end-of-field token and logs the call.
        /// </summary>
        public void ReadFieldEnd()
        {
            _wrapped.ReadFieldEnd();
            Log( "ReadFieldEnd" );
        }

        /// <summary>
        /// Reads a list header and logs the call.
        /// </summary>
        /// <returns>A list header.</returns>
        public ThriftCollectionHeader ReadListHeader()
        {
            var value = _wrapped.ReadListHeader();
            Log( "ReadListHeader", value );
            return value;
        }

        /// <summary>
        /// Reads an end-of-list token and logs the call.
        /// </summary>
        public void ReadListEnd()
        {
            _wrapped.ReadListEnd();
            Log( "ReadListEnd" );
        }

        /// <summary>
        /// Reads a set header and logs the call.
        /// </summary>
        /// <returns>The set header.</returns>
        public ThriftCollectionHeader ReadSetHeader()
        {
            var value = _wrapped.ReadSetHeader();
            Log( "ReadSetHeader", value );
            return value;
        }

        /// <summary>
        /// Reads an end-of-set token and logs the call.
        /// </summary>
        public void ReadSetEnd()
        {
            _wrapped.ReadSetEnd();
            Log( "ReadSetEnd" );
        }

        /// <summary>
        /// Reads a map header and logs the call.
        /// </summary>
        /// <returns>A map header.</returns>
        public ThriftMapHeader ReadMapHeader()
        {
            var value = _wrapped.ReadMapHeader();
            Log( "ReadMapHeader", value );
            return value;
        }

        /// <summary>
        /// Reads an end-of-map token and logs the call.
        /// </summary>
        public void ReadMapEnd()
        {
            _wrapped.ReadMapEnd();
            Log( "ReadMapEnd" );
        }

        /// <summary>
        /// Reads a boolean value and logs the call.
        /// </summary>
        /// <returns>A boolean value.</returns>
        public bool ReadBoolean()
        {
            var value = _wrapped.ReadBoolean();
            Log( "ReadBool", value );
            return value;
        }

        /// <summary>
        /// Reads a signed byte and logs the call.
        /// </summary>
        /// <returns>A signed byte.</returns>
        public sbyte ReadSByte()
        {
            var value = _wrapped.ReadSByte();
            Log( "ReadByte", value );
            return value;
        }

        /// <summary>
        /// Reads an IEEE 754 double-precision floating-point number and logs the call.
        /// </summary>
        /// <returns>An IEEE 754 double-precision floating-point number.</returns>
        public double ReadDouble()
        {
            var value = _wrapped.ReadDouble();
            Log( "ReadDouble", value );
            return value;
        }

        /// <summary>
        /// Reads a 16-bit integer and logs the call.
        /// </summary>
        /// <returns>A 16-bit integer.</returns>
        public short ReadInt16()
        {
            var value = _wrapped.ReadInt16();
            Log( "ReadInt16", value );
            return value;
        }

        /// <summary>
        /// Reads a 32-bit integer and logs the call.
        /// </summary>
        /// <returns>A 32-bit integer.</returns>
        public int ReadInt32()
        {
            var value = _wrapped.ReadInt32();
            Log( "ReadInt32", value );
            return value;
        }

        /// <summary>
        /// Reads a 64-bit integer and logs the call.
        /// </summary>
        /// <returns>A 64-bit integer.</returns>
        public long ReadInt64()
        {
            var value = _wrapped.ReadInt64();
            Log( "ReadInt64", value );
            return value;
        }

        /// <summary>
        /// Reads a string and logs the call.
        /// </summary>
        /// <returns>A string.</returns>
        public string ReadString()
        {
            var value = _wrapped.ReadString();
            Log( "ReadString", "\"" + value + "\"" );
            return value;
        }

        /// <summary>
        /// Reads an array of signed bytes and logs the call.
        /// </summary>
        /// <returns>An array of signed bytes.</returns>
        public sbyte[] ReadBinary()
        {
            var value = _wrapped.ReadBinary();
            Log( "ReadBinary", "[" + string.Join( ",", value ) + "]" );
            return value;
        }


        /// <summary>
        /// Writes the specified message header and logs the call.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteMessageHeader( ThriftMessageHeader header )
        {
            Log( "WriteMessageHeader", header );
            _wrapped.WriteMessageHeader( header );
        }

        /// <summary>
        /// Writes an end-of-message token and logs the call.
        /// </summary>
        public void WriteMessageEnd()
        {
            Log( "WriteMessageEnd" );
            _wrapped.WriteMessageEnd();
        }

        /// <summary>
        /// Writes the specified struct header and logs the call.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteStructHeader( ThriftStructHeader header )
        {
            Log( "WriteStructHeader", header );
            _wrapped.WriteStructHeader( header );
        }

        /// <summary>
        /// Writes an end-of-struct token and logs the call.
        /// </summary>
        public void WriteStructEnd()
        {
            Log( "WriteStructEnd" );
            _wrapped.WriteStructEnd();
        }

        /// <summary>
        /// Writes the specified field header and logs the call.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteFieldHeader( ThriftFieldHeader header )
        {
            Log( "WriteFieldHeader", header );
            _wrapped.WriteFieldHeader( header );
        }

        /// <summary>
        /// Writes an end-of-field token and logs the call.
        /// </summary>
        public void WriteFieldEnd()
        {
            Log( "WriteFieldEnd" );
            _wrapped.WriteFieldEnd();
        }

        /// <summary>
        /// Writes a token signaling the end of fields in a struct and logs the call.
        /// </summary>
        public void WriteFieldStop()
        {
            Log( "WriteFieldStop" );
            _wrapped.WriteFieldStop();
        }

        /// <summary>
        /// Writes the specified list header and logs the call.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteListHeader( ThriftCollectionHeader header )
        {
            Log( "WriteListHeader", header );
            _wrapped.WriteListHeader( header );
        }

        /// <summary>
        /// Writes an end-of-list token and logs the call.
        /// </summary>
        public void WriteListEnd()
        {
            Log( "WriteListEnd" );
            _wrapped.WriteListEnd();
        }

        /// <summary>
        /// Writes the specified set header and logs the call.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteSetHeader( ThriftCollectionHeader header )
        {
            Log( "WriteSetHeader", header );
            _wrapped.WriteSetHeader( header );
        }

        /// <summary>
        /// Writes an end-of-set token and logs the call.
        /// </summary>
        public void WriteSetEnd()
        {
            Log( "WriteSetEnd" );
            _wrapped.WriteSetEnd();
        }

        /// <summary>
        /// Writes the specified map header and logs the call.
        /// </summary>
        /// <param name="header">The header.</param>
        public void WriteMapHeader( ThriftMapHeader header )
        {
            Log( "WriteMapHeader", header );
            _wrapped.WriteMapHeader( header );
        }

        /// <summary>
        /// Writes an end-of-map token and logs the call.
        /// </summary>
        public void WriteMapEnd()
        {
            Log( "WriteMapEnd" );
            _wrapped.WriteMapEnd();
        }

        /// <summary>
        /// Writes the specified boolean value and logs the call.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        public void WriteBoolean( bool value )
        {
            Log( "WriteBool", value );
            _wrapped.WriteBoolean( value );
        }

        /// <summary>
        /// Writes the specified signed byte and logs the call.
        /// </summary>
        /// <param name="value">The signed byte value.</param>
        public void WriteSByte( sbyte value )
        {
            Log( "WriteByte", value );
            _wrapped.WriteSByte( value );
        }

        /// <summary>
        /// Writes the specified IEEE 754 double-precision floating-point number and logs the call.
        /// </summary>
        /// <param name="value">The floating point value.</param>
        public void WriteDouble( double value )
        {
            Log( "WriteDouble", value );
            _wrapped.WriteDouble( value );
        }

        /// <summary>
        /// Writes the specified 16-bit signed integer and logs the call.
        /// </summary>
        /// <param name="value">The integer value.</param>
        public void WriteInt16( short value )
        {
            Log( "WriteInt16", value );
            _wrapped.WriteInt16( value );
        }

        /// <summary>
        /// Writes the specified 32-bit signed integer and logs the call.
        /// </summary>
        /// <param name="value">The integer value.</param>
        public void WriteInt32( int value )
        {
            Log( "WriteInt32", value );
            _wrapped.WriteInt32( value );
        }

        /// <summary>
        /// Writes the specified 64-bit signed integer and logs the call.
        /// </summary>
        /// <param name="value">The integer value.</param>
        public void WriteInt64( long value )
        {
            Log( "WriteInt16", value );
            _wrapped.WriteInt64( value );
        }

        /// <summary>
        /// Writes the specified string and logs the call.
        /// </summary>
        /// <param name="value">The string.</param>
        public void WriteString( string value )
        {
            Log( "WriteString", "\"" + value + "\"" );
            _wrapped.WriteString( value );
        }

        /// <summary>
        /// Writes the specified array of signed bytes and logs the call.
        /// </summary>
        /// <param name="value">The array of signed bytes.</param>
        public void WriteBinary( sbyte[] value )
        {
            Log( "WriteBinary", "[" + string.Join( ",", value ) + "]" );
            _wrapped.WriteBinary( value );
        }
    }
}