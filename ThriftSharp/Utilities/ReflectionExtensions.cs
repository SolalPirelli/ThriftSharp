// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Reflection utility methods and extension methods.
    /// </summary>
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// Gets the specified generic interface definition on the TypeInfo, if it implements it.
        /// </summary>
        public static Type GetGenericInterface( this TypeInfo typeInfo, Type interfaceType )
        {
            if ( typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == interfaceType )
            {
                return typeInfo.AsType();
            }
            return typeInfo.ImplementedInterfaces.FirstOrDefault( i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == interfaceType );
        }

        /// <summary>
        /// Checks whether the TypeInfo extends the specified type.
        /// </summary>
        public static bool Extends( this TypeInfo typeInfo, Type baseType )
        {
            return baseType.GetTypeInfo().IsAssignableFrom( typeInfo );
        }

        /// <summary>
        /// Unwraps a Task if the Type is one, or returns null.
        /// Returns typeof(void) if the Task is not a generic one.
        /// </summary>
        public static Type UnwrapTaskType( Type type )
        {
            var typeInfo = type.GetTypeInfo();
            if ( typeInfo.Extends( typeof( Task ) ) )
            {
                if ( typeInfo.IsGenericType )
                {
                    return typeInfo.GenericTypeArguments[0];
                }
                return typeof( void );
            }
            return null;
        }

        /// <summary>
        /// Creates a new instance of the specified TypeInfo, using a public parameterless constructor.
        /// </summary>
        public static object Create( TypeInfo typeInfo )
        {
            return typeInfo.DeclaredConstructors
                           .First( c => c.GetParameters().Length == 0 )
                           .Invoke( null );
        }
    }
}