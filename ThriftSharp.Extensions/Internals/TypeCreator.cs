﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            var fields = new List<Tuple<string, Method>>();

            // Generate the methods
            foreach ( var m in typeof( T ).GetMethods( BindingFlags.Public | BindingFlags.Instance ) )
            {
                // Create the implementation
                var method = implementor.Invoke( m );
                // Store it in a field
                var fieldBuilder = typeBuilder.DefineField( "_" + m.Name, typeof( Method ), FieldAttributes.Public );
                // Add the field and its future value to the list of fields
                fields.Add( Tuple.Create( fieldBuilder.Name, method ) );
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
                // Remove the returned value if needed
                if ( m.ReturnType == typeof( void ) )
                {
                    gen.Emit( OpCodes.Pop );
                }
                // Unbox it if needed
                else if ( m.ReturnType.IsValueType )
                {
                    gen.Emit( OpCodes.Unbox_Any, m.ReturnType );
                }
                // Return its return value
                gen.Emit( OpCodes.Ret );

                // Required to implement the interface method
                typeBuilder.DefineMethodOverride( methodBuilder, m );
            }

            // Create the instance
            var inst = (T) Activator.CreateInstance( typeBuilder.CreateType() );

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