﻿// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Reads Thrift structs.
    /// </summary>
    internal static class ThriftReader
    {
        // TODO see if actual generics are possible to avoid boxing

        /// <summary>
        /// Asynchronously skips the specified Thrift type ID from the specified protocol.
        /// </summary>
        private static async Task SkipAsync( ThriftTypeId thriftTypeId, IThriftProtocol protocol )
        {
            switch ( thriftTypeId )
            {
                case ThriftTypeId.Boolean:
                    await protocol.ReadBooleanAsync();
                    return;

                case ThriftTypeId.SByte:
                    await protocol.ReadSByteAsync();
                    return;

                case ThriftTypeId.Double:
                    await protocol.ReadDoubleAsync();
                    return;

                case ThriftTypeId.Int16:
                    await protocol.ReadInt16Async();
                    return;

                case ThriftTypeId.Int32:
                    await protocol.ReadInt32Async();
                    return;

                case ThriftTypeId.Int64:
                    await protocol.ReadInt64Async();
                    return;

                case ThriftTypeId.Binary:
                    await protocol.ReadBinaryAsync();
                    return;

                case ThriftTypeId.List:
                    var listHeader = await protocol.ReadListHeaderAsync();
                    for ( int n = 0; n < listHeader.Count; n++ )
                    {
                        await SkipAsync( listHeader.ElementTypeId, protocol );
                    }
                    await protocol.ReadListEndAsync();
                    return;

                case ThriftTypeId.Set:
                    var setHeader = await protocol.ReadSetHeaderAsync();
                    for ( int n = 0; n < setHeader.Count; n++ )
                    {
                        await SkipAsync( setHeader.ElementTypeId, protocol );
                    }
                    await protocol.ReadSetEndAsync();
                    return;

                case ThriftTypeId.Map:
                    var mapHeader = await protocol.ReadMapHeaderAsync();
                    for ( int n = 0; n < mapHeader.Count; n++ )
                    {
                        await SkipAsync( mapHeader.KeyTypeId, protocol );
                        await SkipAsync( mapHeader.ValueTypeId, protocol );
                    }
                    await protocol.ReadMapEndAsync();
                    return;

                case ThriftTypeId.Struct:
                    await protocol.ReadStructHeaderAsync();
                    while ( true )
                    {
                        var fieldHeader = await protocol.ReadFieldHeaderAsync();
                        if ( fieldHeader == null )
                        {
                            break;
                        }
                        await SkipAsync( fieldHeader.FieldTypeId, protocol );
                        await protocol.ReadFieldEndAsync();
                    }
                    await protocol.ReadStructEndAsync();
                    return;
            }
        }

        /// <summary>
        /// Asynchronously reads the specified struct from the specified protocol.
        /// </summary>
        private static async Task<object> ReadStructAsync( ThriftStruct thriftStruct, IThriftProtocol protocol )
        {
            var structInstance = ReflectionEx.Create( thriftStruct.TypeInfo );
            var readIds = new HashSet<short>();

            await protocol.ReadStructHeaderAsync(); // ignore return value
            while ( true )
            {
                var header = await protocol.ReadFieldHeaderAsync();
                if ( header == null )
                {
                    break;
                }


                readIds.Add( header.Id );

                var field = thriftStruct.Fields.FirstOrDefault( f => f.Header.Id == header.Id );
                if ( field == null )
                {
                    await SkipAsync( header.FieldTypeId, protocol );
                }
                else
                {
                    if ( field.Header.FieldTypeId != header.FieldTypeId )
                    {
                        throw ThriftSerializationException.TypeIdMismatch( field.Header.FieldTypeId, header.FieldTypeId );
                    }

                    var fieldValue = await ReadAsync( field.Header.FieldType, protocol );
                    field.SetValue( structInstance, fieldValue );
                }
                await protocol.ReadFieldEndAsync();
            }
            await protocol.ReadStructEndAsync();

            foreach ( var field in thriftStruct.Fields )
            {
                if ( !readIds.Contains( field.Header.Id ) )
                {
                    if ( field.IsRequired )
                    {
                        throw ThriftSerializationException.MissingRequiredField( thriftStruct.Header.Name, field.Header.Name );
                    }
                    if ( field.DefaultValue.HasValue )
                    {
                        field.SetValue( structInstance, field.DefaultValue.Value );
                    }
                }
            }

            return structInstance;
        }

        /// <summary>
        /// Asynchronously reads the specified Thrift type from the specified protocol.
        /// </summary>
        private static async Task<object> ReadAsync( ThriftType thriftType, IThriftProtocol protocol )
        {
            if ( thriftType.IsPrimitive )
            {
                if ( thriftType.IsEnum )
                {
                    return Enum.ToObject( thriftType.TypeInfo.AsType(), await protocol.ReadInt32Async() );
                }
                if ( thriftType.NullableType != null )
                {
                    var ctor = thriftType.TypeInfo.DeclaredConstructors.First( c => c.GetParameters().Length == 1 );
                    return ctor.Invoke( new[] { await ReadAsync( thriftType.NullableType, protocol ) } );
                }

                switch ( thriftType.Id )
                {
                    case ThriftTypeId.Boolean:
                        return await protocol.ReadBooleanAsync();

                    case ThriftTypeId.SByte:
                        return await protocol.ReadSByteAsync();

                    case ThriftTypeId.Double:
                        return await protocol.ReadDoubleAsync();

                    case ThriftTypeId.Int16:
                        return await protocol.ReadInt16Async();

                    case ThriftTypeId.Int32:
                        return await protocol.ReadInt32Async();

                    case ThriftTypeId.Int64:
                        return await protocol.ReadInt64Async();

                    case ThriftTypeId.Binary:
                        if ( thriftType.TypeInfo.AsType() == typeof( string ) )
                        {
                            return await protocol.ReadStringAsync();
                        }
                        return await protocol.ReadBinaryAsync();

                    default:
                        throw new InvalidOperationException( "Assertion error: ThriftType.IsPrimitive must mean ThriftType.Id is a primitive ID." );
                }
            }

            if ( thriftType.CollectionTypeInfo != null )
            {
                bool isArray = thriftType.CollectionTypeInfo.IsArray;

                if ( isArray )
                {
                    var header = await protocol.ReadListHeaderAsync();

                    if ( thriftType.ElementType.Id != header.ElementTypeId )
                    {
                        throw ThriftSerializationException.TypeIdMismatch( thriftType.ElementType.Id, header.ElementTypeId );
                    }

                    var array = Array.CreateInstance( thriftType.ElementType.TypeInfo.AsType(), header.Count );
                    for ( int n = 0; n < header.Count; n++ )
                    {
                        object obj = await ReadAsync( thriftType.ElementType, protocol );
                        array.SetValue( obj, n );
                    }
                    await protocol.ReadListEndAsync();

                    return array;
                }
                else
                {
                    var concreteTypeInfo = CollectionHelper.GetInstantiableVersion( thriftType.CollectionTypeInfo );
                    var instance = ReflectionEx.Create( concreteTypeInfo );
                    var setter = concreteTypeInfo.GetDeclaredMethod( "Add" );

                    var header = thriftType.Id == ThriftTypeId.List ? await protocol.ReadListHeaderAsync()
                                                                    : await protocol.ReadSetHeaderAsync();


                    if ( thriftType.ElementType.Id != header.ElementTypeId )
                    {
                        throw ThriftSerializationException.TypeIdMismatch( thriftType.ElementType.Id, header.ElementTypeId );
                    }

                    for ( int n = 0; n < header.Count; n++ )
                    {
                        object obj = await ReadAsync( thriftType.ElementType, protocol );
                        setter.Invoke( instance, new[] { obj } );
                    }

                    if ( thriftType.Id == ThriftTypeId.List )
                    {
                        await protocol.ReadListEndAsync();
                    }
                    else
                    {
                        await protocol.ReadSetEndAsync();
                    }

                    return instance;
                }
            }

            if ( thriftType.MapTypeInfo != null )
            {
                var header = await protocol.ReadMapHeaderAsync();


                if ( thriftType.KeyType.Id != header.KeyTypeId )
                {
                    throw ThriftSerializationException.TypeIdMismatch( thriftType.KeyType.Id, header.KeyTypeId );
                }
                if ( thriftType.ValueType.Id != header.ValueTypeId )
                {
                    throw ThriftSerializationException.TypeIdMismatch( thriftType.ValueType.Id, header.ValueTypeId );
                }


                var concreteTypeInfo = CollectionHelper.GetInstantiableVersion( thriftType.MapTypeInfo );
                var instance = ReflectionEx.Create( concreteTypeInfo );
                var setter = concreteTypeInfo.GetDeclaredMethod( "Add" );

                for ( int n = 0; n < header.Count; n++ )
                {
                    object key = await ReadAsync( thriftType.KeyType, protocol );
                    object value = await ReadAsync( thriftType.ValueType, protocol );
                    setter.Invoke( instance, new[] { key, value } );
                }
                await protocol.ReadMapEndAsync();

                return instance;
            }

            return await ReadStructAsync( thriftType.Struct, protocol );
        }


        /// <summary>
        /// Asynchronously reads the specified struct from the specified protocol.
        /// </summary>
        public static Task<object> ReadAsync( ThriftStruct thriftStruct, IThriftProtocol protocol )
        {
            return ReadStructAsync( thriftStruct, protocol );
        }
    }
}