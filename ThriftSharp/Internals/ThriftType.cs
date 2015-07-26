// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThriftSharp.Models;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Thrift type, either a primitive, a collection or a struct.
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

        private static readonly Dictionary<Type, ThriftType> _knownTypes = new Dictionary<Type, ThriftType>();


        // Generic arguments if the type is a list, set or map
        private readonly Type[] _collectionGenericArgs;


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
        /// Gets the type of the collection elements, if the type is a list or set.
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
        /// Initializes a new instance of the ThriftType class from the specified .NET type.
        /// </summary>
        private ThriftType( Type type )
        {
            Id = ThriftTypeId.Empty;
            TypeInfo = type.GetTypeInfo();

            if ( type == typeof( void ) )
            {
                return;
            }

            var underlyingNullableType = Nullable.GetUnderlyingType( type );
            if ( underlyingNullableType != null )
            {
                NullableType = ThriftType.Get( underlyingNullableType );
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

            if ( TypeInfo.IsArray )
            {
                Id = ThriftTypeId.List;
                return;
            }

            var mapInterfaceAndArgs = GetInstantiableVersion( TypeInfo, typeof( IDictionary<,> ), typeof( Dictionary<,> ), ThriftParsingException.UnsupportedMap );
            if ( mapInterfaceAndArgs != null )
            {
                Id = ThriftTypeId.Map;
                TypeInfo = mapInterfaceAndArgs.Item1;
                _collectionGenericArgs = mapInterfaceAndArgs.Item2;
            }

            var setInterfaceAndArgs = GetInstantiableVersion( TypeInfo, typeof( ISet<> ), typeof( HashSet<> ), ThriftParsingException.UnsupportedSet );
            if ( setInterfaceAndArgs != null )
            {
                if ( mapInterfaceAndArgs != null )
                {
                    throw ThriftParsingException.CollectionWithOrthogonalInterfaces( TypeInfo );
                }

                Id = ThriftTypeId.Set;
                TypeInfo = setInterfaceAndArgs.Item1;
                _collectionGenericArgs = setInterfaceAndArgs.Item2;
            }

            var listInterfaceAndArgs = GetInstantiableVersion( TypeInfo, typeof( IList<> ), typeof( List<> ), ThriftParsingException.UnsupportedList );
            if ( listInterfaceAndArgs != null )
            {
                if ( mapInterfaceAndArgs != null || setInterfaceAndArgs != null )
                {
                    throw ThriftParsingException.CollectionWithOrthogonalInterfaces( TypeInfo );
                }

                Id = ThriftTypeId.List;
                TypeInfo = listInterfaceAndArgs.Item1;
                _collectionGenericArgs = listInterfaceAndArgs.Item2;
            }

            if ( Id == ThriftTypeId.Empty )
            {
                Id = ThriftTypeId.Struct;
            }
        }

        /// <summary>
        /// Gets the Thrift wire type associated with the specified type.
        /// </summary>
        public static ThriftType Get( Type type )
        {
            if ( !_knownTypes.ContainsKey( type ) )
            {
                var thriftType = new ThriftType( type );

                _knownTypes.Add( type, thriftType );

                // This has to be done this way because otherwise self-referencing types will loop 
                // since they'd call ThriftType.Get before they were themselves added to _knownTypes
                switch ( thriftType.Id )
                {
                    case ThriftTypeId.Map:
                        thriftType.KeyType = ThriftType.Get( thriftType._collectionGenericArgs[0] );
                        thriftType.ValueType = ThriftType.Get( thriftType._collectionGenericArgs[1] );
                        break;

                    case ThriftTypeId.List:
                    case ThriftTypeId.Set:
                        if ( thriftType.TypeInfo.IsArray )
                        {
                            thriftType.ElementType = ThriftType.Get( thriftType.TypeInfo.GetElementType() );
                        }
                        else
                        {
                            thriftType.ElementType = ThriftType.Get( thriftType._collectionGenericArgs[0] );
                        }
                        break;
                }
            }

            return _knownTypes[type];
        }

        /// <summary>
        /// Gets an instantiable version of the specified type, for the specified interface type,
        /// with the specified concrete type if needed, throwing the specified exception if the type 
        /// does implement the interface but cannot be instantiated.
        /// Returns null if the type does not implement the interface.
        /// </summary>
        private static Tuple<TypeInfo, Type[]> GetInstantiableVersion( TypeInfo typeInfo, Type interfaceType, Type concreteType, Func<TypeInfo, Exception> errorProvider )
        {
            if ( typeInfo.IsInterface )
            {
                if ( typeInfo.GenericTypeArguments.Length > 0 )
                {
                    var unbound = typeInfo.GetGenericTypeDefinition();
                    if ( unbound == interfaceType )
                    {
                        return Tuple.Create(
                            concreteType.MakeGenericType( typeInfo.GenericTypeArguments ).GetTypeInfo(),
                            typeInfo.GenericTypeArguments
                        );
                    }
                }
            }

            Tuple<TypeInfo, Type[]> instantiableVersion = null;
            foreach ( var iface in typeInfo.ImplementedInterfaces.Where( i => i.GenericTypeArguments.Length > 0 ) )
            {
                var unboundIface = iface.GetGenericTypeDefinition();
                if ( unboundIface == interfaceType )
                {
                    if ( typeInfo.IsAbstract || !typeInfo.DeclaredConstructors.Any( c => c.GetParameters().Length == 0 ) )
                    {
                        throw errorProvider( typeInfo );
                    }

                    if ( instantiableVersion != null )
                    {
                        throw ThriftParsingException.CollectionWithMultipleGenericImplementations( typeInfo );
                    }

                    instantiableVersion = Tuple.Create( typeInfo, iface.GenericTypeArguments );
                }
            }

            return instantiableVersion;
        }
    }
}