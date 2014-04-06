// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Reflection utility methods and extension methods.
    /// </summary>
    internal static class ReflectionEx
    {
        /// <summary>
        /// Gets the attribute of the specified type on the MemberInfo, or null if there is no such attribute.
        /// </summary>
        public static T GetAttribute<T>( this MemberInfo info )
            where T : Attribute
        {
            return (T) info.GetCustomAttributes( typeof( T ) ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the attribute of the specified type on the ParameterInfo, or null if there is no such attribute.
        /// </summary>
        /// <remarks>
        /// This is required since ParameterInfo does not inherit from MemberInfo.
        /// </remarks>
        public static T GetAttribute<T>( this ParameterInfo info )
            where T : Attribute
        {
            return (T) info.GetCustomAttributes( typeof( T ) ).FirstOrDefault();
        }

        /// <summary>
        /// Gets all attributes of the specified type on the MemberInfo.
        /// </summary>
        public static IEnumerable<T> GetAttributes<T>( this MemberInfo info )
        {
            return info.GetCustomAttributes( typeof( T ) ).Cast<T>();
        }

        /// <summary>
        /// Gets the value of the enum member.
        /// </summary>
        public static int GetEnumMemberValue( this FieldInfo info )
        {
            return (int) info.GetValue( null );
        }

        /// <summary>
        /// Gets the specified generic interface definition on the TypeInfo, if it implements it.
        /// </summary>
        public static Type GetGenericInterface( this TypeInfo typeInfo, Type interfaceType )
        {
            return typeInfo.ImplementedInterfaces.FirstOrDefault( i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == interfaceType );
        }

        /// <summary>
        /// Gets the interface property with the specified name.
        /// </summary>
        public static PropertyInfo GetInterfaceProperty( this TypeInfo typeInfo, string name )
        {
            return typeInfo.ImplementedInterfaces
                           .Concat( new[] { typeInfo.AsType() } )
                           .Select( i => i.GetTypeInfo().GetDeclaredProperty( name ) )
                           .First( p => p != null );
        }

        /// <summary>
        /// Unwraps a Task if the Type is one, or returns null.
        /// Returns typeof(void) if the Task is not a generic one.
        /// </summary>
        public static Type UnwrapTask( Type type )
        {
            var typeInfo = type.GetTypeInfo();
            if ( typeof( Task ).GetTypeInfo().IsAssignableFrom( typeInfo ) )
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
        /// Creates a new instance of the specified TypeInfo, using a public constructor.
        /// </summary>
        public static object Create( TypeInfo typeInfo )
        {
            return typeInfo.DeclaredConstructors
                           .First( c => c.GetParameters().Length == 0 )
                           .Invoke( null );
        }
    }
}