// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Reflection;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    internal sealed class ThriftType
    {
        public readonly ThriftTypeId Id;
        public readonly TypeInfo TypeInfo;

        public readonly bool IsPrimitive;
        public readonly bool IsNullable;
        public readonly bool IsEnum;

        public readonly TypeInfo CollectionTypeInfo;
        public readonly ThriftType ElementType;

        public readonly TypeInfo MapTypeInfo;
        public readonly ThriftType KeyType;
        public readonly ThriftType ValueType;


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

        private static readonly IDictionary<Type, ThriftTypeId> PrimitiveIds = new Dictionary<Type, ThriftTypeId>
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