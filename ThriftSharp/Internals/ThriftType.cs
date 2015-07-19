// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Thrift type, either a primitive, a collection, a map or a struct.
    /// </summary>
    internal sealed class ThriftType
    {
        private static readonly Dictionary<Type, ThriftTypeId> PrimitiveIds = new Dictionary<Type, ThriftTypeId>
        {
            { typeof( bool ), ThriftTypeId.Boolean },
            { typeof( sbyte ), ThriftTypeId.SByte },
            { typeof( double ), ThriftTypeId.Double },
            { typeof( short ), ThriftTypeId.Int16 },
            { typeof( int ), ThriftTypeId.Int32 },
            { typeof( long ), ThriftTypeId.Int64 },
            { typeof( string ), ThriftTypeId.Binary },
            { typeof( sbyte[] ), ThriftTypeId.Binary }
        };

        private static readonly Dictionary<Type, Type> CollectionImplementations = new Dictionary<Type, Type>
        {
            { typeof( ISet<> ), typeof( HashSet<> ) },
            { typeof( ICollection<> ), typeof( List<> ) },
            { typeof( IList<> ), typeof( List<> ) },
            { typeof( IDictionary<,> ), typeof( Dictionary<,> ) }
        };

        // Known .NET types to Thrift types mappings
        private static readonly Dictionary<Type, ThriftType> _knownTypes = new Dictionary<Type, ThriftType>();

        /// <summary>
        /// Type of the interface for collections (including maps).
        /// </summary>
        private readonly TypeInfo _collectionTypeInfo;

        /// <summary>
        /// Gets the type's ID.
        /// </summary>
        public ThriftTypeId Id { get; private set; }

        /// <summary>
        /// Gets the TypeInfo of the underlying type.
        /// </summary>
        public TypeInfo TypeInfo { get; private set; }

        /// <summary>
        /// Gets the underlying type, if the type is a nullable type.
        /// </summary>
        public ThriftType NullableType { get; private set; }

        /// <summary>
        /// Gets the type of the collection elements, if the type is a collection.
        /// </summary>
        public ThriftType ElementType { get; private set; }

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

            if ( type == typeof( void ) )
            {
                Id = ThriftTypeId.Empty;
                return;
            }

            var underlyingNullableType = Nullable.GetUnderlyingType( type );
            if ( underlyingNullableType != null )
            {
                NullableType = ThriftType.Get( underlyingNullableType, null );
                Id = NullableType.Id;
                return;
            }

            if ( PrimitiveIds.ContainsKey( type ) )
            {
                Id = PrimitiveIds[type];
                return;
            }

            if ( TypeInfo.IsEnum )
            {
                if ( TypeInfo.GetCustomAttribute<ThriftEnumAttribute>() == null )
                {
                    throw ThriftParsingException.EnumWithoutAttribute( TypeInfo );
                }
                if ( Enum.GetUnderlyingType( type ) != typeof( int ) )
                {
                    throw ThriftParsingException.NonInt32Enum( TypeInfo );
                }

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
                var instantiableVersion = GetInstantiableVersion( TypeInfo );
                if ( instantiableVersion == null )
                {
                    throw ThriftParsingException.UnsupportedMap( TypeInfo );
                }

                Id = ThriftTypeId.Map;
                TypeInfo = instantiableVersion;
                _collectionTypeInfo = mapInterface.GetTypeInfo();
                return;
            }

            var setInterface = TypeInfo.GetGenericInterface( typeof( ISet<> ) );
            if ( setInterface != null )
            {
                var instantiableVersion = GetInstantiableVersion( TypeInfo );
                if ( instantiableVersion == null )
                {
                    throw ThriftParsingException.UnsupportedSet( TypeInfo );
                }

                Id = ThriftTypeId.Set;
                TypeInfo = instantiableVersion;
                _collectionTypeInfo = setInterface.GetTypeInfo();
                return;
            }

            var collectionInterface = TypeInfo.GetGenericInterface( typeof( ICollection<> ) );
            if ( collectionInterface != null )
            {
                var instantiableVersion = GetInstantiableVersion( TypeInfo );
                if ( instantiableVersion == null )
                {
                    throw ThriftParsingException.UnsupportedList( TypeInfo );
                }

                Id = ThriftTypeId.List;
                TypeInfo = instantiableVersion;
                _collectionTypeInfo = collectionInterface.GetTypeInfo();
                return;
            }

            Id = ThriftTypeId.Struct;
        }

        /// <summary>
        /// Gets the Thrift wire type associated with the specified type and converter.
        /// </summary>
        public static ThriftType Get( Type type, object converter )
        {
            if ( converter != null )
            {
                type = converter.GetType().GetTypeInfo().GetGenericInterface( typeof( IThriftValueConverter<,> ) ).GenericTypeArguments[0];
                var nullableType = Nullable.GetUnderlyingType( type );
                if ( nullableType != null )
                {
                    type = typeof( Nullable<> ).MakeGenericType( new[] { type } );
                }
            }


            if ( !_knownTypes.ContainsKey( type ) )
            {
                var thriftType = new ThriftType( type );

                _knownTypes.Add( type, thriftType );

                // This has to be done this way because otherwise self-referencing types will loop 
                // since they'd call ThriftType.Get before they were themselves added to _knownTypes
                switch ( thriftType.Id )
                {
                    case ThriftTypeId.Map:
                        thriftType.KeyType = ThriftType.Get( thriftType._collectionTypeInfo.GenericTypeArguments[0], null );
                        thriftType.ValueType = ThriftType.Get( thriftType._collectionTypeInfo.GenericTypeArguments[1], null );
                        break;

                    case ThriftTypeId.List:
                    case ThriftTypeId.Set:
                        if ( thriftType.TypeInfo.IsArray )
                        {
                            thriftType.ElementType = ThriftType.Get( thriftType.TypeInfo.GetElementType(), null );
                        }
                        else
                        {
                            thriftType.ElementType = ThriftType.Get( thriftType._collectionTypeInfo.GenericTypeArguments[0], null );
                        }
                        break;

                    case ThriftTypeId.Struct:
                        thriftType.Struct = ThriftAttributesParser.ParseStruct( thriftType.TypeInfo );
                        break;
                }
            }

            return _knownTypes[type];
        }

        /// <summary>
        /// Maps the specified TypeInfo to a TypeInfo that can be instantiated with a parameterless constructor, or returns null if that is not possible.
        /// </summary>
        private static TypeInfo GetInstantiableVersion( TypeInfo typeInfo )
        {
            if ( typeInfo.IsArray || ( !typeInfo.IsAbstract && !typeInfo.IsInterface && typeInfo.DeclaredConstructors.Any( c => c.GetParameters().Length == 0 ) ) )
            {
                return typeInfo;
            }
            else if ( typeInfo.IsGenericType && CollectionImplementations.ContainsKey( typeInfo.GetGenericTypeDefinition() ) )
            {
                return CollectionImplementations[typeInfo.GetGenericTypeDefinition()].MakeGenericType( typeInfo.GenericTypeArguments ).GetTypeInfo();
            }
            else
            {
                return null;
            }
        }
    }
}