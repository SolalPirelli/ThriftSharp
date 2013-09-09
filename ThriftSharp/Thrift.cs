using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp
{
    /// <summary>
    /// Main entry point to Thrift#.
    /// Translates Thrift interface definitions into protocol calls.
    /// </summary>
    public static class Thrift
    {
        /// <summary>
        /// Creates a read-only ThriftField with the specified header and value.
        /// </summary>
        private static ThriftField ReadOnlyField( ThriftFieldHeader header, Type underlyingType, object value )
        {
            return new ThriftField( header, true, new Option<object>(), underlyingType, _ => value, null );
        }

        /// <summary>
        /// Creates a ThriftStruct representing the specified parameters of the specified method.
        /// </summary>
        private static ThriftStruct MakeParametersStruct( ThriftMethod method, object[] args )
        {
            var paramFields = new ThriftField[method.Parameters.Count];
            for ( int n = 0; n < method.Parameters.Count; n++ )
            {
                var param = method.Parameters[n];
                var type = ThriftSerializer.FromType( param.UnderlyingParameter.ParameterType );
                var header = new ThriftFieldHeader( param.Id, param.Name, type.ThriftType );
                paramFields[n] = ReadOnlyField( header, param.UnderlyingParameter.ParameterType, args[n] );
            }

            return new ThriftStruct( new ThriftStructHeader( "Parameters" ), paramFields );
        }

        /// <summary>
        /// Calls the specified ThriftMethod on the specified protocol with the specified arguments.
        /// </summary>
        private static void CallMethod( IThriftProtocol protocol, ThriftMethod method, object[] args )
        {
            var msg = new ThriftMessageHeader( 0, method.Name, method.IsOneWay ? ThriftMessageType.Call : ThriftMessageType.OneWay );
            var paramSt = MakeParametersStruct( method, args );

            protocol.WriteMessageHeader( msg );
            ThriftSerializer.WriteStruct( protocol, paramSt, null );
            protocol.WriteMessageEnd();
        }


        /// <summary>
        /// Creates a set-only ThriftField with the specified header and values.
        /// </summary>
        private static ThriftField SetOnlyField( ThriftFieldHeader header, bool isRequired, Type underlyingType, Action<object> setter )
        {
            return new ThriftField( header, isRequired, new Option<object>(), underlyingType, null, ( _, v ) => setter( v ) );
        }

        /// <summary>
        /// Creates a ThriftStruct representing the return type of the specified ThriftMethod.
        /// </summary>
        private static Tuple<ThriftStruct, Container> MakeReturnStruct( ThriftMethod method )
        {
            var retFields = new List<ThriftField>();
            var retValContainer = new Container();

            if ( method.ReturnType != typeof( void ) )
            {
                var retType = ThriftSerializer.FromType( method.ReturnType );
                var retHeader = new ThriftFieldHeader( 0, "ReturnType", retType.ThriftType );
                retFields.Add( SetOnlyField( retHeader, true, method.ReturnType, v => retValContainer.Value = v ) );
            }

            foreach ( var e in method.Exceptions )
            {
                var header = new ThriftFieldHeader( e.Id, e.Name, ThriftSerializer.Struct.ThriftType );
                retFields.Add( SetOnlyField( header, false, e.ExceptionType, v => { throw (Exception) v; } ) );
            }

            return Tuple.Create( new ThriftStruct( new ThriftStructHeader( "Parameters" ), retFields ), retValContainer );
        }

        /// <summary>
        /// Reads a protocol exception from the specified protocol.
        /// </summary>
        private static ThriftProtocolException ReadException( IThriftProtocol protocol )
        {
            // Server exception (not a declared one)
            var exn = ThriftSerializer.Struct.Read( protocol, typeof( ThriftProtocolException ) );
            protocol.ReadMessageEnd();
            return (ThriftProtocolException) exn;
        }

        /// <summary>
        /// Reads a ThriftMessage returned by the specified ThriftMethod on the specified ThriftProtocol.
        /// </summary>
        private static object ReadMessage( IThriftProtocol protocol, ThriftMethod method )
        {
            var header = protocol.ReadMessageHeader();

            if ( !EnumEx.GetValues<ThriftMessageType>().Contains( header.MessageType ) )
            {
                throw new ThriftProtocolException( ThriftProtocolExceptionType.InvalidMessageType, "The returned Thrift message type is invalid." );
            }
            if ( header.MessageType == ThriftMessageType.Exception )
            {
                throw ReadException( protocol );
            }

            var retStAndVal = MakeReturnStruct( method );
            ThriftSerializer.ReadStruct( protocol, retStAndVal.Item1, null );
            protocol.ReadMessageEnd();

            return retStAndVal.Item2.Value;
        }


        /// <summary>
        /// Sends a Thrift message representing the specified method call with the specified arguments on the specified protocol.
        /// </summary>
        internal static object SendMessage( IThriftProtocol protocol, ThriftMethod method, params object[] args )
        {
            Func<object> action = () =>
            {
                CallMethod( protocol, method, args );
                return ReadMessage( protocol, method );
            };

            if ( method.IsAsync )
            {
                return Task.Factory.StartNew( action );
            }
            return action();
        }


        /// <summary>
        /// Calls a ThriftMethod specified by its name with the specified arguments on the specified protocol.
        /// </summary>
        /// <typeparam name="TService">The type of the Thrift service.</typeparam>
        /// <param name="protocol">The protocol.</param>
        /// <param name="methodName">The method name.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The method result.</returns>
        public static object CallMethod<TService>( IThriftProtocol protocol, string methodName, params object[] args )
        {
            var method = ThriftAttributesParser.ParseService( typeof( TService ) ).Methods.FirstOrDefault( m => m.UnderlyingName == methodName );
            if ( method == null )
            {
                throw new ArgumentException( string.Format( "Invalid method name ({0})", methodName ) );
            }
            return SendMessage( protocol, method, args );
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