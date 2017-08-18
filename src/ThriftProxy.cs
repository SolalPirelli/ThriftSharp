using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ThriftSharp.Internals;
using ThriftSharp.Utilities;

namespace ThriftSharp
{
    /// <summary>
    /// Dynamically creates interface implementations for Thrift interfaces.
    /// </summary>
    public sealed class ThriftProxy
    { 
        /// <summary>
        /// Creates a proxy for the specified interface type using the specified protocol.
        /// </summary>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <param name="communication">The means of communication with the server.</param>
        /// <returns>A dynamically generated proxy to the Thrift service.</returns>
        public static T Create<T>( ThriftCommunication communication )
        {
            var proxy = DispatchProxy.Create<T, Implementation>();

            var proxyAsImpl = (Implementation) (object) proxy;
            proxyAsImpl.Communication = communication;
            proxyAsImpl.Service = ThriftAttributesParser.ParseService( typeof( T ).GetTypeInfo() );

            return proxy;
        }

        /// <summary>
        /// Calls the specified method with the specified arguments using the specified means of communication.
        /// </summary>
        /// <param name="communication">The means of communication with the server.</param>
        /// <param name="service">The Thrift service containing the method.</param>
        /// <param name="methodName">The .NET method name.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The method result.</returns>
        internal static async Task<T> CallMethodAsync<T>( ThriftCommunication communication, ThriftService service, string methodName, params object[] args )
        {
            // The attributes parser guarantees that there are 0 or 1 tokens per method
            var token = args.OfType<CancellationToken>().FirstOrDefault();

            using( var protocol = communication.CreateProtocol( token ) )
            {
                var method = service.Methods[methodName];
                var methodArgs = args.Where( a => !( a is CancellationToken ) ).ToArray();

                for( int n = 0; n < methodArgs.Length; n++ )
                {
                    if( methodArgs[n] == null )
                    {
                        throw ThriftSerializationException.NullParameter( method.Parameters[n].Name );
                    }
                }

                ThriftClientMessageWriter.Write( method, methodArgs, protocol );

                await protocol.FlushAndReadAsync();

                if( method.IsOneWay )
                {
                    return default( T );
                }

                return ThriftClientMessageReader.Read<T>( method, protocol );
            }
        }


        /// <summary>
        /// Infrastructure.
        /// Do not use this class.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public class Implementation : DispatchProxy
        {
            private static readonly MethodInfo CallAsyncGenericMethod =
                typeof( ThriftProxy ).GetTypeInfo().DeclaredMethods.First( m => m.Name == nameof( ThriftProxy.CallMethodAsync ) );


            internal ThriftCommunication Communication { get; set; }

            internal ThriftService Service { get; set; }


            /// <summary>
            /// Infrastructure.
            /// Do not use this method.
            /// </summary>
            /// <param name="targetMethod">Undocumented.</param>
            /// <param name="args">Undocumented.</param>
            /// <returns>Undocumented.</returns>
            protected override object Invoke( MethodInfo targetMethod, object[] args )
            {
                var returnType = ReflectionExtensions.UnwrapTask( targetMethod.ReturnType );
                if( returnType == typeof( void ) )
                {
                    returnType = typeof( object );
                }

                var method = CallAsyncGenericMethod.MakeGenericMethod( returnType );

                return method.Invoke( null, new object[] { Communication, Service, targetMethod.Name, args } );
            }
        }
    }
}