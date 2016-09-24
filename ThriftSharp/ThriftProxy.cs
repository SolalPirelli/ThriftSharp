#if NET_FULL
using System;
using System.Reflection.Emit;
using System.Threading.Tasks;
#endif
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ThriftSharp.Internals;
using ThriftSharp.Utilities;

namespace ThriftSharp
{
    /// <summary>
    /// Dynamically creates interface implementations for Thrift interfaces.
    /// </summary>
    public sealed class ThriftProxy
    {
#if NET_FULL
        /// <summary>
        /// Infrastructure.
        /// Do not use this type.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public static class SpecialProxy
        {
            /// <summary>
            /// Infrastructure.
            /// Do not call this method.
            /// </summary>
            /// <typeparam name="T">Undocumented.</typeparam>
            /// <param name="communication">Undocumented.</param>
            /// <param name="thriftService">Undocumented.</param>
            /// <param name="methodName">Undocumented.</param>
            /// <param name="args">Undocumented.</param>
            /// <returns>Undocumented.</returns>
            [EditorBrowsable( EditorBrowsableState.Never )]
            [Obsolete( "Do not use this method.", true )]
            public static Task<T> CallMethodAsync<T>( ThriftCommunication communication, object thriftService, string methodName, object[] args )
            {
                return Thrift.CallMethodAsync<T>( communication, (ThriftService) thriftService, methodName, args );
            }
        }

        // Attributes for all generated public methods
        private const MethodAttributes GeneratedMethodAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;

        // We need to call Thrift.CallMethodAsync<T>, which is internal, from a generated assembly,
        // thus we have to use a public proxy method, which takes an 'object' instead of a 'ThriftService' since the latter is also internal.
        private static MethodInfo CallMethodAsyncMethod = typeof( SpecialProxy ).GetTypeInfo().GetDeclaredMethod( "CallMethodAsync" );

        /// <summary>
        /// Creates a proxy for the specified interface type using the specified protocol.
        /// </summary>
        /// <typeparam name="T">The interface type.</typeparam>
        /// <param name="communication">The means of communication with the server.</param>
        /// <returns>A dynamically generated proxy to the Thrift service.</returns>
        public static T Create<T>( ThriftCommunication communication )
        {
            // N.B. this code makes a lot of assumptions specific to ThriftSharp, it can't just be reused to build any proxy

            // Get the service type
            var service = ThriftAttributesParser.ParseService( typeof( T ).GetTypeInfo() );

            // Create a dynamic assembly
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly( new AssemblyName( "GeneratedCode" ), AssemblyBuilderAccess.Run );
            // Add a module
            var moduleBuilder = assemblyBuilder.DefineDynamicModule( "Module" );
            // Add a type inside that module
            var typeBuilder = moduleBuilder.DefineType( typeof( T ).Name, TypeAttributes.Public );
            // Make the type inherit from the interface
            typeBuilder.AddInterfaceImplementation( typeof( T ) );
            // Add a field for the "communication" parameter of this method
            var commField = typeBuilder.DefineField( "communication", typeof( ThriftCommunication ), FieldAttributes.Public );
            // Add a field for the "service" parameter of this method
            // whose type is object for reasons detailed below
            var serviceField = typeBuilder.DefineField( "service", typeof( object ), FieldAttributes.Public );

            // Generate the methods
            foreach( var m in typeof( T ).GetMethods( BindingFlags.Public | BindingFlags.Instance ) )
            {
                // Define the method parameter types
                var methodParamTypes = m.GetParameters().Select( p => p.ParameterType ).ToArray();
                // Create the method
                var methodBuilder = typeBuilder.DefineMethod( m.Name, GeneratedMethodAttributes, m.ReturnType, methodParamTypes );
                // Get the method's parameters
                var parameters = m.GetParameters();

                // Generate the IL..
                var gen = methodBuilder.GetILGenerator();

                // Create a new local variable, which will be an array
                var paramsLocal = gen.DeclareLocal( typeof( object[] ) );
                // Load the array length
                gen.Emit( OpCodes.Ldc_I4, m.GetParameters().Length );
                // Create the array (using the length above)
                gen.Emit( OpCodes.Newarr, typeof( object ) );
                // Store the created array in the local
                gen.Emit( OpCodes.Stloc, paramsLocal );

                // For each parameter:
                for( int n = 0; n < parameters.Length; n++ )
                {
                    // Load the array
                    gen.Emit( OpCodes.Ldloc, paramsLocal );
                    // Load the index of the parameter in the array
                    gen.Emit( OpCodes.Ldc_I4, n );
                    // Load the parameter, +1 because it's an instance method thus arg 0 is the instance
                    gen.Emit( OpCodes.Ldarg, n + 1 );
                    // Box it if needed
                    if( parameters[n].ParameterType.IsValueType )
                    {
                        gen.Emit( OpCodes.Box, parameters[n].ParameterType );
                    }
                    // Set the parameter at the index in the array
                    gen.Emit( OpCodes.Stelem_Ref );
                }

                // Get the return type for the CallMethodAsync method, i.e. the unwrapped return type...
                var unwrappedReturnType = m.ReturnType.UnwrapTask();
                if( unwrappedReturnType == typeof( void ) )
                {
                    // ... or object if there is none
                    unwrappedReturnType = typeof( object );
                }

                // Get the CallMethodAsync method
                var proxiedMethod = CallMethodAsyncMethod.MakeGenericMethod( unwrappedReturnType );

                // Load the instance to load a field
                gen.Emit( OpCodes.Ldarg_0 );
                // Load the first argument of CallMethodAsync (the communication)
                gen.Emit( OpCodes.Ldfld, commField );
                // Load the instance to load a field
                gen.Emit( OpCodes.Ldarg_0 );
                // Load the second argument of CallMethodAsync (the service, whose type is object as mentioned above)
                gen.Emit( OpCodes.Ldfld, serviceField );
                // Load the third argument of CallMethodAsync (the method name)
                gen.Emit( OpCodes.Ldstr, m.Name );
                // Load the fourth and final argument of CallMethodAsync (the arguments)
                gen.Emit( OpCodes.Ldloc, paramsLocal );
                // Call the method
                gen.Emit( OpCodes.Call, proxiedMethod );
                // Return its return value
                gen.Emit( OpCodes.Ret );

                // Required to implement the interface method
                typeBuilder.DefineMethodOverride( methodBuilder, m );
            }

            // Get the instance type
            var instanceTypeInfo = typeBuilder.CreateTypeInfo();
            // Create the instance
            var instance = (T) Activator.CreateInstance( instanceTypeInfo.AsType() );

            // Set the "service" field
            instanceTypeInfo.GetDeclaredField( serviceField.Name ).SetValue( instance, service );
            // Set the "communication" field
            instanceTypeInfo.GetDeclaredField( commField.Name ).SetValue( instance, communication );

            // Return the instance.
            return instance;
        }
#else
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
        /// Infrastructure.
        /// Do not use this class.
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public class Implementation : DispatchProxy
        {
            private static readonly MethodInfo CallAsyncGenericMethod =
                typeof( Thrift ).GetTypeInfo().DeclaredMethods.First( m => m.Name == nameof( Thrift.CallMethodAsync ) );


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
#endif
    }
}