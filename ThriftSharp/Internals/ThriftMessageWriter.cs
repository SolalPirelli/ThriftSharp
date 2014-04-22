// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Reflection;
using System.Threading.Tasks;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Writes Thrift messages from a client.
    /// </summary>
    internal static class ThriftMessageWriter
    {
        /// <summary>
        /// Creates a read-only ThriftField with the specified header and value.
        /// </summary>
        private static ThriftField ReadOnlyField( short id, string name, TypeInfo typeInfo, object value )
        {
            return new ThriftField( id, name, true, new Option(), typeInfo, _ => value, null );
        }

        /// <summary>
        /// Creates a ThriftStruct representing the specified parameters of the specified method.
        /// </summary>
        private static ThriftStruct MakeParametersStruct( ThriftMethod method, object[] args )
        {
            if ( args.Length != method.Parameters.Count )
            {
                throw new ArgumentException( string.Format( "Parameter count mismatch. Expected {0}, got {1}.", method.Parameters.Count, args.Length ) );
            }

            var paramFields = new ThriftField[method.Parameters.Count];
            for ( int n = 0; n < method.Parameters.Count; n++ )
            {
                var param = method.Parameters[n];
                if ( param.Converter == null )
                {
                    paramFields[n] = ReadOnlyField( param.Id, param.Name, param.TypeInfo, args[n] );
                }
                else
                {
                    paramFields[n] = ReadOnlyField( param.Id, param.Name, param.Converter.FromType.GetTypeInfo(), param.Converter.ConvertBack( args[n] ) );
                }
            }

            return new ThriftStruct( new ThriftStructHeader( "" ), paramFields, typeof( object ).GetTypeInfo() );
        }

        /// <summary>
        /// Calls the specified ThriftMethod on the specified protocol with the specified arguments.
        /// </summary>
        public static Task WriteAsync( IThriftProtocol protocol, ThriftMethod method, object[] args )
        {
            var msg = new ThriftMessageHeader( 0, method.Name, method.IsOneWay ? ThriftMessageType.OneWay : ThriftMessageType.Call );
            var paramSt = MakeParametersStruct( method, args );

            protocol.WriteMessageHeader( msg );
            ThriftWriter.Write( paramSt, null, protocol );
            protocol.WriteMessageEnd();
            return protocol.FlushAsync();
        }
    }
}