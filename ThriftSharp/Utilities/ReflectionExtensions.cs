// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Reflection utility extension methods.
    /// </summary>
    internal static class ReflectionExtensions
    {
        /// <summary>
        /// Gets all implementations of the specified generic interface definitions on the TypeInfo.
        /// </summary>
        public static TypeInfo[] GetGenericInterfaces( this TypeInfo typeInfo, Type interfaceType )
        {
            return typeInfo.ImplementedInterfaces
                           .Where( i => i.GenericTypeArguments.Length > 0 && i.GetGenericTypeDefinition() == interfaceType )
                           .Select( t => t.GetTypeInfo() )
                           .ToArray();
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
        public static Type UnwrapTask( this Type type )
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
    }
}