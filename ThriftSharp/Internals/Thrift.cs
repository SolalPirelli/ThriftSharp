// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Single entry point to Thrift#'s internals.
    /// Translates Thrift interface definitions into protocol calls.
    /// </summary>
    internal static class Thrift
    {
        /// <summary>
        /// Creates a read-only ThriftField with the specified header and value.
        /// </summary>
        private static ThriftField ReadOnlyField( short id, string name, TypeInfo typeInfo, object value )
        {
            return new ThriftField( id, name, true, new Option<object>(), typeInfo, _ => value, null );
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
                var type = ThriftSerializer.FromTypeInfo( param.TypeInfo );

                if ( param.Converter == null )
                {
                    paramFields[n] = ReadOnlyField( param.Id, param.Name, param.TypeInfo, args[n] );
                }
                else
                {
                    paramFields[n] = ReadOnlyField( param.Id, param.Name, param.Converter.FromType.GetTypeInfo(), param.Converter.ConvertBack( args[n] ) );
                }
            }

            return new ThriftStruct( new ThriftStructHeader( "" ), paramFields );
        }

        /// <summary>
        /// Calls the specified ThriftMethod on the specified protocol with the specified arguments.
        /// </summary>
        internal static void CallMethod( IThriftProtocol protocol, ThriftMethod method, object[] args )
        {
            var msg = new ThriftMessageHeader( 0, method.Name, method.IsOneWay ? ThriftMessageType.OneWay : ThriftMessageType.Call );
            var paramSt = MakeParametersStruct( method, args );

            protocol.WriteMessageHeader( msg );
            ThriftSerializer.WriteStruct( protocol, paramSt, null );
            protocol.WriteMessageEnd();
        }


        /// <summary>
        /// Creates a set-only ThriftField with the specified header and values.
        /// </summary>
        private static ThriftField SetOnlyField( short id, string name, bool isRequired, TypeInfo typeInfo, Action<object> setter )
        {
            return new ThriftField( id, name, isRequired, new Option<object>(), typeInfo, null, ( _, v ) => setter( v ) );
        }

        /// <summary>
        /// Creates a ThriftStruct representing the return type of the specified ThriftMethod.
        /// </summary>
        private static Tuple<ThriftStruct, Container> MakeReturnStruct( ThriftMethod method )
        {
            var retFields = new List<ThriftField>();
            var retValContainer = new Container();

            if ( method.ReturnTypeInfo.AsType() != typeof( void ) )
            {
                if ( method.ReturnValueConverter == null )
                {
                    var retType = ThriftSerializer.FromTypeInfo( method.ReturnTypeInfo );
                    retFields.Add( SetOnlyField( 0, "", true, method.ReturnTypeInfo, v => retValContainer.Value = v ) );
                }
                else
                {
                    var retTypeInfo = method.ReturnValueConverter.FromType.GetTypeInfo();
                    var retType = ThriftSerializer.FromTypeInfo( retTypeInfo );
                    retFields.Add( SetOnlyField( 0, "", true, retTypeInfo,
                                                 v => retValContainer.Value = method.ReturnValueConverter.Convert( v ) ) );
                }
            }

            foreach ( var e in method.Exceptions )
            {
                retFields.Add( SetOnlyField( e.Id, e.Name, false, e.ExceptionTypeInfo, v => { throw (Exception) v; } ) );
            }

            return Tuple.Create( new ThriftStruct( new ThriftStructHeader( "" ), retFields ), retValContainer );
        }

        /// <summary>
        /// Reads a protocol exception from the specified protocol.
        /// </summary>
        private static async Task<ThriftProtocolException> ReadExceptionAsync( IThriftProtocol protocol )
        {
            // Server exception (not a declared one)
            var exceptionTypeInfo = typeof( ThriftProtocolException ).GetTypeInfo();
            var exception = await ThriftSerializer.FromTypeInfo( exceptionTypeInfo ).ReadAsync( protocol, exceptionTypeInfo );
            await protocol.ReadMessageEndAsync();
            return (ThriftProtocolException) exception;
        }

        /// <summary>
        /// Reads a ThriftMessage returned by the specified ThriftMethod on the specified ThriftProtocol.
        /// </summary>
        private static async Task<object> ReadMessageAsync( IThriftProtocol protocol, ThriftMethod method )
        {
            var header = await protocol.ReadMessageHeaderAsync();

            if ( !EnumEx.GetValues<ThriftMessageType>().Contains( header.MessageType ) )
            {
                throw new ThriftProtocolException( ThriftProtocolExceptionType.InvalidMessageType, "The returned Thrift message type is invalid." );
            }
            if ( header.MessageType == ThriftMessageType.Exception )
            {
                throw await ReadExceptionAsync( protocol );
            }

            var retStAndVal = MakeReturnStruct( method );
            await ThriftSerializer.ReadStructAsync( protocol, retStAndVal.Item1, null );
            await protocol.ReadMessageEndAsync();
            // Dispose of it now that we have finished reading and writing
            // using() is quite dangerous in this case because of async stuff happening
            protocol.Dispose();

            return retStAndVal.Item2.Value;
        }


        /// <summary>
        /// Sends a Thrift message representing the specified method call with the specified arguments on the specified protocol, and gets the result.
        /// </summary>
        private static Task<object> SendMessageAsync( IThriftProtocol protocol, ThriftMethod method, params object[] args )
        {
            CallMethod( protocol, method, args );
            return ReadMessageAsync( protocol, method );
        }


        /// <summary>
        /// Calls a ThriftMethod specified by its name with the specified arguments using the specified means of communication.
        /// </summary>
        /// <param name="communication">The means of communication with the server.</param>
        /// <param name="service">The Thrift service containing the method.</param>
        /// <param name="methodName">The underlying method name.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The method result.</returns>
        public static async Task<T> CallMethodAsync<T>( ThriftCommunication communication, ThriftService service, string methodName, params object[] args )
        {
            var protocol = communication.CreateProtocol();
            var method = service.Methods.FirstOrDefault( m => m.UnderlyingName == methodName );
            if ( method == null )
            {
                throw new ArgumentException( string.Format( "Invalid method name ({0})", methodName ) );
            }
            return (T) await SendMessageAsync( protocol, method, args );
        }


        /// <summary>
        /// Utility class that holds a single value.
        /// </summary>
        private sealed class Container
        {
            public object Value { get; set; }
        }
    }
}