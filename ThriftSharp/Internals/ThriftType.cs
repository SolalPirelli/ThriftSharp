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

        // Known .NET types to Thrift types mappings
        private static readonly Dictionary<Type, ThriftType> _knownTypes = new Dictionary<Type, ThriftType>();


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
        /// Gets a value indicating whether the underlying type is an enum.
        /// </summary>
        public readonly bool IsEnum;


        /// <summary>
        /// Gets the underlying type, if the type is a nullable type.
        /// </summary>
        public readonly ThriftType NullableType;


        /// <summary>
        /// Gets the TypeInfo of the type as a collection, if it is a collection.
        /// </summary>
        public readonly TypeInfo CollectionTypeInfo;

        /// <summary>
        /// Gets the type of the collection elements, if the type is a collection.
        /// </summary>
        public ThriftType ElementType { get; private set; }


        /// <summary>
        /// Gets the TypeInfo of the type as a map, if it is a map.
        /// </summary>
        public readonly TypeInfo MapTypeInfo;

        /// <summary>
        /// Gets the type of the map keys, if the type is a map.
        /// </summary>
        public ThriftType KeyType { get; private set; }

        /// <summary>
        /// Gets the type of the map values, if the type is a map.
        /// </summary>
        public ThriftType ValueType { get; private set; }


        /// <summary>
        /// Gets the Thrift type of the struct, if it is a struct.
        /// </summary>
        public ThriftStruct Struct { get; private set; }


        /// <summary>
        /// Creates a new instance of ThriftType from the specified .NET type.
        /// </summary>
        private ThriftType( Type type )
        {
            TypeInfo = type.GetTypeInfo();

            Type underlyingNullableType = Nullable.GetUnderlyingType( type );
            if ( underlyingNullableType != null )
            {
                NullableType = ThriftType.Get( underlyingNullableType );
                IsPrimitive = true;
                Id = NullableType.Id;
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
                if ( TypeInfo.GetAttribute<ThriftEnumAttribute>() == null )
                {
                    throw ThriftParsingException.EnumWithoutAttribute( TypeInfo );
                }
                if ( Enum.GetUnderlyingType( type ) != typeof( int ) )
                {
                    throw ThriftParsingException.NonInt32Enum( TypeInfo );
                }

                IsEnum = true;
                IsPrimitive = true;
                Id = ThriftTypeId.Int32;
                return;
            }

            if ( TypeInfo.IsValueType )
            {
                throw ThriftParsingException.UnknownValueType( TypeInfo );
            }

            var mapInterface = TypeInfo.GetGenericInterface( typeof( IDictionary<,> ) );
            if ( mapInterface != null )
            {
                if ( !CollectionHelper.CanBeMapped( TypeInfo ) )
                {
                    throw ThriftParsingException.UnsupportedMap( TypeInfo );
                }

                Id = ThriftTypeId.Map;
                MapTypeInfo = mapInterface.GetTypeInfo();
                return;
            }

            var setInterface = TypeInfo.GetGenericInterface( typeof( ISet<> ) );
            if ( setInterface != null )
            {
                if ( !CollectionHelper.CanBeMapped( TypeInfo ) )
                {
                    throw ThriftParsingException.UnsupportedSet( TypeInfo );
                }

                Id = ThriftTypeId.Set;
                CollectionTypeInfo = setInterface.GetTypeInfo();
                return;
            }

            var collectionInterface = TypeInfo.GetGenericInterface( typeof( ICollection<> ) );
            if ( collectionInterface != null )
            {
                if ( !CollectionHelper.CanBeMapped( TypeInfo ) )
                {
                    throw ThriftParsingException.UnsupportedList( TypeInfo );
                }

                Id = ThriftTypeId.List;

                if ( TypeInfo.IsArray )
                {
                    CollectionTypeInfo = TypeInfo;
                }
                else
                {
                    CollectionTypeInfo = collectionInterface.GetTypeInfo();
                }
                return;
            }

            Id = ThriftTypeId.Struct;
        }

        /// <summary>
        /// Gets the Thrift type associated with the specified type.
        /// </summary>
        public static ThriftType Get( Type type )
        {
            if ( !_knownTypes.ContainsKey( type ) )
            {
                var thriftType = new ThriftType( type );

                _knownTypes.Add( type, thriftType );

                // This has to be done this way because otherwise self-referencing types 
                // (e.g. type A with a field of type A) will loop since they're calling 
                // ThriftType.Get before they were themselves added to _knownTypes
                switch ( thriftType.Id )
                {
                    case ThriftTypeId.Map:
                        thriftType.KeyType = ThriftType.Get( thriftType.MapTypeInfo.GenericTypeArguments[0] );
                        thriftType.ValueType = ThriftType.Get( thriftType.MapTypeInfo.GenericTypeArguments[1] );
                        break;

                    case ThriftTypeId.List:
                    case ThriftTypeId.Set:
                        if ( thriftType.TypeInfo.IsArray )
                        {
                            thriftType.ElementType = ThriftType.Get( thriftType.TypeInfo.GetElementType() );
                        }
                        else
                        {
                            thriftType.ElementType = ThriftType.Get( thriftType.CollectionTypeInfo.GenericTypeArguments[0] );
                        }
                        break;

                    case ThriftTypeId.Struct:
                        thriftType.Struct = ThriftAttributesParser.ParseStruct( thriftType.TypeInfo );
                        break;
                }
            }

            return _knownTypes[type];
        }
    }
}