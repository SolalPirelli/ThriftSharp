// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Utility class to get concrete types from collection types.
    /// </summary>
    internal static class KnownCollections
    {
        private static readonly Dictionary<Type, Type> Implementations = new Dictionary<Type, Type>
        {
            { typeof( ISet<> ), typeof( HashSet<> ) },
            { typeof( ICollection<> ), typeof( List<> ) },
            { typeof( IList<> ), typeof( List<> ) },
            { typeof( IDictionary<,> ), typeof( Dictionary<,> ) }
        };


        /// <summary>
        /// Gets a value indicating whether the specified TypeInfo can be mapped to an instantiable TypeInfo.
        /// </summary>
        public static bool CanBeMapped( TypeInfo typeInfo )
        {
            return typeInfo.IsArray
                || ( typeInfo.IsGenericType && Implementations.ContainsKey( typeInfo.GetGenericTypeDefinition() ) )
                || ( !typeInfo.IsAbstract && !typeInfo.IsInterface && typeInfo.DeclaredConstructors.Any( c => c.GetParameters().Length == 0 ) );
        }

        /// <summary>
        /// Maps the specified TypeInfo to a TypeInfo that can be instantiated with a parameterless constructor.
        /// </summary>
        /// <remarks>
        /// Assumes that CanBeMapped(typeInfo) is true.
        /// </remarks>
        public static TypeInfo GetInstantiableVersion( TypeInfo typeInfo )
        {
            if ( typeInfo.IsInterface )
            {
                return Implementations[typeInfo.GetGenericTypeDefinition()].MakeGenericType( typeInfo.GenericTypeArguments ).GetTypeInfo();
            }
            return typeInfo;
        }
    }
}