using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Reads Thrift messages from a server.
    /// </summary>
    internal static class ThriftMessageReader
    {
        /// <summary>
        /// Creates a set-only ThriftField with the specified header and values.
        /// </summary>
        private static ThriftField SetOnlyField( short id, string name, TypeInfo typeInfo, Action<object> setter )
        {
            return new ThriftField( id, name, false, new Option(), typeInfo, null, ( _, v ) => setter( v ) );
        }

        /// <summary>
        /// Creates a ThriftStruct representing the return type of the specified ThriftMethod.
        /// </summary>
        private static Tuple<ThriftStruct, Option> MakeReturnStruct( ThriftMethod method )
        {
            var fields = new List<ThriftField>();
            var returnOption = new Option();

            if ( method.ReturnType != typeof( void ) )
            {
                if ( method.ReturnValueConverter == null )
                {
                    fields.Add( SetOnlyField( 0, "", method.ReturnType.GetTypeInfo(), v => returnOption.Value = v ) );
                }
                else
                {
                    fields.Add( SetOnlyField( 0, "", method.ReturnValueConverter.FromType.GetTypeInfo(),
                                              v => returnOption.Value = method.ReturnValueConverter.Convert( v ) ) );
                }
            }

            foreach ( var e in method.Exceptions )
            {
                fields.Add( SetOnlyField( e.Id, e.Name, e.ExceptionTypeInfo, v => { throw (Exception) v; } ) );
            }

            return Tuple.Create( new ThriftStruct( new ThriftStructHeader( "" ), fields, typeof( object ).GetTypeInfo() ), returnOption );
        }

        /// <summary>
        /// Reads a protocol exception from the specified protocol.
        /// </summary>
        private static async Task<ThriftProtocolException> ReadExceptionAsync( IThriftProtocol protocol )
        {
            // Server exception (not a declared one)
            var exceptionStruct = ThriftAttributesParser.ParseStruct( typeof( ThriftProtocolException ).GetTypeInfo() );
            var exception = await ThriftReader.ReadAsync( exceptionStruct, protocol );
            await protocol.ReadMessageEndAsync();
            return (ThriftProtocolException) exception;
        }

        /// <summary>
        /// Reads a ThriftMessage returned by the specified ThriftMethod on the specified ThriftProtocol.
        /// </summary>
        public static async Task<object> ReadAsync( IThriftProtocol protocol, ThriftMethod method )
        {
            var header = await protocol.ReadMessageHeaderAsync();

            if ( !Enum.IsDefined( typeof( ThriftMessageType ), header.MessageType ) )
            {
                throw new ThriftProtocolException( ThriftProtocolExceptionType.InvalidMessageType, "The returned Thrift message type is invalid." );
            }
            if ( header.MessageType == ThriftMessageType.Exception )
            {
                throw await ReadExceptionAsync( protocol );
            }

            var retStAndVal = MakeReturnStruct( method );
            await ThriftReader.ReadAsync( retStAndVal.Item1, protocol );
            await protocol.ReadMessageEndAsync();
            // Dispose of it now that we have finished reading and writing
            // using() is quite dangerous in this case because of async stuff happening
            protocol.Dispose();

            if ( retStAndVal.Item2.HasValue )
            {
                return retStAndVal.Item2.Value;
            }
            if ( method.ReturnType == typeof( void ) )
            {
                return null;
            }
            throw new ThriftProtocolException( ThriftProtocolExceptionType.MissingResult, "The result is missing." );
        }
    }
}