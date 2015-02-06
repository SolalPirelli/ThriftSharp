// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThriftSharp.Protocols;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Single entry point to Thrift#'s internals.
    /// Translates Thrift interface definitions into protocol calls.
    /// </summary>
    internal static class Thrift
    {
        /// <summary>
        /// Sends a Thrift message representing the specified method call with the specified arguments on the specified protocol, and gets the result.
        /// </summary>
        private static async Task<object> SendMessageAsync( IThriftProtocol protocol, ThriftMethod method, params object[] args )
        {
            ThriftMessageWriter.Write( protocol, method, args );
            await protocol.FlushAndReadAsync();

            if ( method.IsOneWay )
            {
                return null;
            }

            return ThriftMessageReader.Read( protocol, method );
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
            var method = service.Methods.FirstOrDefault( m => m.UnderlyingName == methodName );
            if ( method == null )
            {
                throw new ArgumentException( string.Format( "Invalid method name ({0})", methodName ) );
            }

            var token = args.OfType<CancellationToken>().FirstOrDefault();
            var protocol = communication.CreateProtocol( token );

            var methodArgs = args.Where( a => !( a is CancellationToken ) ).ToArray();

            return (T) await SendMessageAsync( protocol, method, methodArgs ).ConfigureAwait( false );
        }
    }
}