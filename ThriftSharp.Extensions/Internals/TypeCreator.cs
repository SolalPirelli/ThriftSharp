// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using ThriftSharp.Utilities;
// A big ugly, but declaring a delegate means declaring a public one (for the invoke to work) in an internals namespace, which is worse.
using Method = System.Func<object[], object>;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Receives a MethodInfo and returns an implementation of that method.
    /// </summary>
    internal delegate Func<object[], object> MethodImplementor( MethodInfo mi );

    /// <summary>
    /// A dynamic type creator that uses Reflection.Emit to build interface implementations at runtime.
    /// </summary>
    internal static class TypeCreator
    {
        /// <summary>
        /// Creates an implementation of the specified interface, using the specified method implementor.
        /// </summary>
        public static T CreateImplementation<T>( MethodImplementor implementor )
        {
            // Create a dynamic assembly
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly( new AssemblyName( "GeneratedCode" ), AssemblyBuilderAccess.Run );
            // Add a module
            var moduleBuilder = assemblyBuilder.DefineDynamicModule( "MainModule" );
            // Add a type inside that module
            var typeBuilder = moduleBuilder.DefineType( typeof( T ).Name, TypeAttributes.Public );
            // Make the type inherit from the interface
            typeBuilder.AddInterfaceImplementation( typeof( T ) );

            // Store the field names and their values to set them later, once we've created the type.
            var fields = new List<Tuple<string, object>>();

            // Generate the methods
            foreach ( var m in typeof( T ).GetMethods( BindingFlags.Public | BindingFlags.Instance ) )
            {
                // Create the implementation
                var method = implementor.Invoke( m );
                // Store it in a field
                var fieldBuilder = typeBuilder.DefineField( "_Implementation_" + m.Name, typeof( Method ), FieldAttributes.Public );
                // Add the field and its future value to the list of fields
                fields.Add( Tuple.Create( fieldBuilder.Name, (object) method ) );
                // The above steps are needed to invoke a delegate from IL; 
                // otherwise it's in the generating assembly and the generated assembly can't access it.

                // Define the method attributes
                var methodAttrs = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
                // Define the return type
                var methodRetType = m.ReturnType == typeof( void ) ? null : m.ReturnType;
                // Define the method parameter types
                var methodParamTypes = m.GetParameters().Select( p => p.ParameterType ).ToArray();
                // Create the method
                var methodBuilder = typeBuilder.DefineMethod( m.Name, methodAttrs, methodRetType, methodParamTypes );

                // Generate the IL..
                var gen = methodBuilder.GetILGenerator();
                // Create a new local variable, which will be an array
                var localBuilder = gen.DeclareLocal( typeof( object[] ) );
                // Load the array length
                gen.Emit( OpCodes.Ldc_I4, m.GetParameters().Length );
                // Create the array (using the length above)
                gen.Emit( OpCodes.Newarr, typeof( object ) );
                // Put it in the local variables list
                gen.Emit( OpCodes.Stloc, localBuilder );

                var parameters = m.GetParameters();
                for ( int n = 0; n < parameters.Length; n++ )
                {
                    // For each parameter:
                    var p = parameters[n];
                    // Load the array
                    gen.Emit( OpCodes.Ldloc, localBuilder );
                    // Load the index of the parameter in the array
                    gen.Emit( OpCodes.Ldc_I4, n );
                    // Load the parameter, +1 because it's an instance method thus arg 0 is the instance
                    gen.Emit( OpCodes.Ldarg, n + 1 );
                    // Box it if needed
                    if ( p.ParameterType.IsValueType )
                    {
                        gen.Emit( OpCodes.Box, p.ParameterType );
                    }
                    // Set the parameter at the index in the array
                    gen.Emit( OpCodes.Stelem_Ref );
                }

                // Load the instance
                gen.Emit( OpCodes.Ldarg_0 );
                // Load the delegate field
                gen.Emit( OpCodes.Ldfld, fieldBuilder );
                // Load the delegate argument
                gen.Emit( OpCodes.Ldloc, localBuilder );
                // Call the delegate
                gen.Emit( OpCodes.Call, typeof( Method ).GetMethod( "Invoke" ) );
                // Cast its wrapped return value
                var unwrapped = ReflectionEx.UnwrapTask( m.ReturnType );
                if ( unwrapped != typeof( void ) )
                {
                    // Create a method that converts a Task<object> into its Result type casted correctly
                    var dynMeth = new DynamicMethod( "_TaskCast_" + m.Name, unwrapped, new[] { typeof( Task<object> ) } );
                    // Generate another set of IL instructions. Yay!
                    var mGen = dynMeth.GetILGenerator();
                    // Load the first argument
                    mGen.Emit( OpCodes.Ldarg_0 );
                    // Get the "Result" property
                    mGen.Emit( OpCodes.Callvirt, typeof( Task<object> ).GetProperty( "Result" ).GetGetMethod() );
                    // If it's a value type, unbox it; otherwise cast it (same opcode)
                    mGen.Emit( OpCodes.Unbox_Any, unwrapped );
                    // Return the casted value
                    mGen.Emit( OpCodes.Ret );
                    // Build the method delegate type
                    var dynDelType = typeof( Func<,> ).MakeGenericType( typeof( Task<object> ), unwrapped );
                    // Finish creating the method
                    var dynDel = dynMeth.CreateDelegate( dynDelType );
                    // Create a field with the dynamic method
                    var fieldCastBuilder = typeBuilder.DefineField( "_TaskCast" + m.Name, dynDelType, FieldAttributes.Public );
                    // Add the field to the list of fields to be set on the instance
                    fields.Add( Tuple.Create( fieldCastBuilder.Name, (object) dynDel ) );

                    // Load the instance
                    gen.Emit( OpCodes.Ldarg_0 );
                    // Load the field that contains the delegate
                    gen.Emit( OpCodes.Ldfld, fieldCastBuilder );
                    // Get the "ContinueWith" method (this seems to be the only way; GetMethod fails to find a proper overload for generic methods)
                    var continueWithMethod = typeof( Task<object> ).GetMethods()
                                                                   .First( mi => mi.Name == "ContinueWith"
                                                                        && mi.IsGenericMethod
                                                                       // first param's first generic param is generic
                                                                       // i.e. it's the overload taking a Func<Task<...>,...>
                                                                        && mi.GetParameters()[0].ParameterType.GetGenericArguments()[0].IsGenericType )
                                                                   .MakeGenericMethod( unwrapped );
                    // Call the "ContinueWith" method with the delegate
                    gen.Emit( OpCodes.Callvirt, continueWithMethod );
                }
                // Return its return value
                gen.Emit( OpCodes.Ret );

                // Required to implement the interface method
                typeBuilder.DefineMethodOverride( methodBuilder, m );
            }

            // Create the instance
            var inst = (T) ReflectionEx.Create( typeBuilder.CreateType().GetTypeInfo() );

            // Set the fields (delegates) values
            foreach ( var tup in fields )
            {
                // For some reason we can't just keep the FieldBuilder from before; its SetValue method doesn't work.
                inst.GetType().GetField( tup.Item1 ).SetValue( inst, tup.Item2 );
            }

            // Return the instance
            return inst;
        }
    }
}