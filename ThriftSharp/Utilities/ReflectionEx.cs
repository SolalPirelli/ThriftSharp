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
        {
            return (T) info.GetCustomAttributes( typeof( T ), true ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the attribute of the specified type on the ParameterInfo, or null if there is no such attribute.
        /// </summary>
        /// <remarks>
        /// This is required since ParameterInfo does not inherit from MemberInfo.
        /// </remarks>
        public static T GetAttribute<T>( this ParameterInfo info )
        {
            return (T) info.GetCustomAttributes( typeof( T ), true ).FirstOrDefault();
        }

        /// <summary>
        /// Gets all attributes of the specified type on the MemberInfo.
        /// </summary>
        public static IEnumerable<T> GetAttributes<T>( this MemberInfo info )
        {
            return info.GetCustomAttributes( typeof( T ), true ).Cast<T>();
        }

        /// <summary>
        /// Gets the value of the enum member.
        /// </summary>
        public static int GetEnumMemberValue( this FieldInfo info )
        {
            return (int) info.GetValue( null );
        }

        /// <summary>
        /// Gets the specified generic interface definition on the Type, if it implements it.
        /// </summary>
        public static Type GetGenericInterface( this Type type, Type interfaceType )
        {
            return type.GetInterfaces().FirstOrDefault( i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType );
        }

        /// <summary>
        /// Unwraps a Task if the Type is one, or returns null.
        /// Returns typeof(void) if the Task is not a generic one.
        /// </summary>
        public static Type UnwrapTaskIfNeeded( Type type )
        {
            if ( typeof( Task ).IsAssignableFrom( type ) )
            {
                if ( type.IsGenericType )
                {
                    return type.GetGenericArguments()[0];
                }
                return typeof( void );
            }
            return null;
        }

        /// <summary>
        /// Gets the "Add" method of the specified collection interface type on the specified type.
        /// </summary>
        public static MethodInfo GetAddMethod( Type type, Type interfaceType )
        {
            return type.GetMethod( "Add", type.GetGenericInterface( interfaceType ).GetGenericArguments() );
        }

        /// <summary>
        /// Creates a new instance of a type, using a public or non-public constructor.
        /// </summary>
        public static object Create( Type type )
        {
            return type.GetConstructors( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                       .First( c => c.GetParameters().Length == 0 )
                       .Invoke( null );
        }
    }
}