// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// A dynamic type creator that uses Reflection.Emit to build interface implementations at runtime.
    /// </summary>
    internal static class TypeCreator
    {
        /// <summary>
        /// Creates an implementation of the specified interface, using the specified method implementor.
        /// </summary>
        /// <remarks>
        /// This makes quite a few assumptions specific to Thrift#, e.g. it only handles <see cref="Task" /> and <see cref="Task{T}" /> return types.
        /// </remarks>
        public static T CreateImplementation<T>( ThriftCommunication communication, ThriftService service )
        {
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
            foreach ( var m in typeof( T ).GetMethods( BindingFlags.Public | BindingFlags.Instance ) )
            {
                // Define the method attributes
                var methodAttrs = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
                // Define the method parameter types
                var methodParamTypes = m.GetParameters().Select( p => p.ParameterType ).ToArray();
                // Create the method
                var methodBuilder = typeBuilder.DefineMethod( m.Name, methodAttrs, m.ReturnType, methodParamTypes );
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
                for ( int n = 0; n < parameters.Length; n++ )
                {
                    // Load the array
                    gen.Emit( OpCodes.Ldloc, paramsLocal );
                    // Load the index of the parameter in the array
                    gen.Emit( OpCodes.Ldc_I4, n );
                    // Load the parameter, +1 because it's an instance method thus arg 0 is the instance
                    gen.Emit( OpCodes.Ldarg, n + 1 );
                    // Box it if needed
                    if ( parameters[n].ParameterType.IsValueType )
                    {
                        gen.Emit( OpCodes.Box, parameters[n].ParameterType );
                    }
                    // Set the parameter at the index in the array
                    gen.Emit( OpCodes.Stelem_Ref );
                }

                // Get the return type for the CallMethodAsync method, i.e. the unwrapped return type...
                var unwrappedReturnType = ReflectionEx.UnwrapTask( m.ReturnType );
                if ( unwrappedReturnType == typeof( void ) )
                {
                    // ... or object if there is none
                    unwrappedReturnType = typeof( object );
                }

                // Now we have a problem: We need to call Thrift.CallMethodAsync<T>, which is internal...
                // so we have to use a public proxy method, which takes an 'object' instead of a 'ThriftService'
                // since the latter is also internal.

                // Get the CallMethodAsync method
                var proxiedMethod = typeof( SpecialProxy ).GetMethods()
                                                          .First( tm => tm.Name == "CallMethodAsync" )
                                                          .MakeGenericMethod( unwrappedReturnType );
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
            var instanceType = typeBuilder.CreateType();
            // Create the instance
            var instance = (T) ReflectionEx.Create( instanceType.GetTypeInfo() );

            // Set the "service" field
            instanceType.GetField( serviceField.Name ).SetValue( instance, service );
            // Set the "communication" field
            instanceType.GetField( commField.Name ).SetValue( instance, communication );

            // Return the instance.
            return instance;
        }
    }
}