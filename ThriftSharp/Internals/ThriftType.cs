// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Thrift type, either a primitive, a collection, a map or a struct.
    /// </summary>
    internal sealed class ThriftType
    {
        /// <summary>
        /// Gets the type's ID.
        /// </summary>
        public readonly ThriftTypeId Id;

        /// <summary>
        /// Gets the TypeInfo of the underlying type.
        /// </summary>
        public readonly TypeInfo TypeInfo;


        /// <summary>
        /// Gets a value indicating whether the type is a primitive.
        /// </summary>
        public readonly bool IsPrimitive;

        /// <summary>
        /// Gets a value indicating whether the underlying type is a nullable type.
        /// </summary>
        public readonly bool IsNullable;

        /// <summary>
        /// Gets a value indicating whether the underlying type is an enum.
        /// </summary>
        public readonly bool IsEnum;


        /// <summary>
        /// Gets the TypeInfo of the type as a collection, if it is a collection.
        /// </summary>
        public readonly TypeInfo CollectionTypeInfo;

        /// <summary>
        /// Gets the type of the collection elements, if the type is a collection.
        /// </summary>
        public readonly ThriftType ElementType;


        /// <summary>
        /// Gets the TypeInfo of the type as a map, if it is a map.
        /// </summary>
        public readonly TypeInfo MapTypeInfo;

        /// <summary>
        /// Gets the type of the map keys, if the type is a map.
        /// </summary>
        public readonly ThriftType KeyType;

        /// <summary>
        /// Gets the type of the map values, if the type is a map.
        /// </summary>
        public readonly ThriftType ValueType;


        /// <summary>
        /// Creates a new instance of ThriftType from the specified .NET type.
        /// </summary>
        public ThriftType( Type type )
        {
            TypeInfo = type.GetTypeInfo();

            if ( TypeInfo.IsGenericType && TypeInfo.GetGenericTypeDefinition() == typeof( Nullable<> ) )
            {
                IsNullable = true;
                IsPrimitive = true;
                Id = PrimitiveIds[TypeInfo.GenericTypeArguments[0]];
                return;
            }

            if ( PrimitiveIds.ContainsKey( type ) )
            {
                IsPrimitive = true;
                Id = PrimitiveIds[type];
                return;
            }

            if ( TypeInfo.IsEnum )
            {
                IsEnum = true;
                IsPrimitive = true;
                Id = ThriftTypeId.Int32;
                return;
            }

            var mapInterface = TypeInfo.GetGenericInterface( typeof( IDictionary<,> ) );
            if ( mapInterface != null )
            {
                Id = ThriftTypeId.Map;
                MapTypeInfo = mapInterface.GetTypeInfo();
                KeyType = new ThriftType( mapInterface.GenericTypeArguments[0] );
                ValueType = new ThriftType( mapInterface.GenericTypeArguments[1] );
                return;
            }

            var setInterface = TypeInfo.GetGenericInterface( typeof( ISet<> ) );
            if ( setInterface != null )
            {
                Id = ThriftTypeId.Set;
                CollectionTypeInfo = setInterface.GetTypeInfo();
                ElementType = new ThriftType( setInterface.GenericTypeArguments[0] );
                return;
            }

            var collectionInterface = TypeInfo.GetGenericInterface( typeof( ICollection<> ) );
            if ( collectionInterface != null )
            {
                Id = ThriftTypeId.List;

                if ( TypeInfo.IsArray )
                {
                    CollectionTypeInfo = TypeInfo;
                    ElementType = new ThriftType( TypeInfo.GetElementType() );
                }
                else
                {
                    CollectionTypeInfo = collectionInterface.GetTypeInfo();
                    ElementType = new ThriftType( collectionInterface.GenericTypeArguments[0] );
                }
                return;
            }

            Id = ThriftTypeId.Struct;
        }


        // Maps .NET types to Thrift type IDs for the Thrift primitive types
        private static readonly Dictionary<Type, ThriftTypeId> PrimitiveIds = new Dictionary<Type, ThriftTypeId>
        {
            { typeof(bool), ThriftTypeId.Boolean },
            { typeof(sbyte), ThriftTypeId.SByte },
            { typeof(double), ThriftTypeId.Double },
            { typeof(short), ThriftTypeId.Int16 },
            { typeof(int), ThriftTypeId.Int32 },
            { typeof(long), ThriftTypeId.Int64 },
            { typeof(string), ThriftTypeId.Binary },
            { typeof(sbyte[]), ThriftTypeId.Binary }
        };
    }
}