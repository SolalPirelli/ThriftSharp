// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Utility class to get concrete types from collection types.
    /// </summary>
    public static class CollectionHelper
    {
        // Known interface collection implementations
        private static readonly Dictionary<Type, Type> KnownImplementations = new Dictionary<Type, Type>
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
                || ( typeInfo.IsGenericType && KnownImplementations.ContainsKey( typeInfo.GetGenericTypeDefinition() ) )
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
                return KnownImplementations[typeInfo.GetGenericTypeDefinition()].MakeGenericType( typeInfo.GenericTypeArguments ).GetTypeInfo();
            }
            return typeInfo;
        }
    }
}