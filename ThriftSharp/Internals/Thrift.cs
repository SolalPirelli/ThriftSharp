// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Single entry point to Thrift#'s internals.
    /// </summary>
    internal static class Thrift
    {
        /// <summary>
        /// Calls the specified method with the specified arguments using the specified means of communication.
        /// </summary>
        /// <param name="communication">The means of communication with the server.</param>
        /// <param name="service">The Thrift service containing the method.</param>
        /// <param name="methodName">The .NET method name.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The method result.</returns>
        public static async Task<T> CallMethodAsync<T>( ThriftCommunication communication, ThriftService service, string methodName, params object[] args )
        {
            // The attributes parser guarantees that there are 0 or 1 tokens per method
            var token = args.OfType<CancellationToken>().FirstOrDefault();
            var protocol = communication.CreateProtocol( token );
            var method = service.Methods[methodName];
            var methodArgs = args.Where( a => !( a is CancellationToken ) ).ToArray();

            for ( int n = 0; n < methodArgs.Length; n++ )
            {
                if ( methodArgs[n] == null )
                {
                    throw ThriftSerializationException.NullParameter( method.Parameters[n].Name );
                }
            }

            ThriftClientMessageWriter.Write( method, methodArgs, protocol );

            await protocol.FlushAndReadAsync();

            if ( method.IsOneWay )
            {
                protocol.Dispose();
                return default( T );
            }

            return ThriftClientMessageReader.Read<T>( method, protocol );
        }
    }
}