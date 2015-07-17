// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Reads Thrift structs.
    /// </summary>
    internal static class ThriftReader
    {
        // Cached common values
        private static class Cache
        {
            public static readonly TypeInfo StringTypeInfo = typeof( string ).GetTypeInfo();
        }

        // Empty Types array, widely used in expression trees
        private static readonly Type[] EmptyTypes = new Type[0];


        // Cached compiled readers
        private static readonly Dictionary<ThriftStruct, Func<IThriftProtocol, object>> _knownReaders
            = new Dictionary<ThriftStruct, Func<IThriftProtocol, object>>();


        /// <summary>
        /// Skips the specified <see cref="ThriftTypeId"/> from the specified <see cref="IThriftProtocol"/>.
        /// </summary>
        private static void Skip( ThriftTypeId thriftTypeId, IThriftProtocol protocol )
        {
            switch ( thriftTypeId )
            {
                case ThriftTypeId.Boolean:
                    protocol.ReadBoolean();
                    return;

                case ThriftTypeId.SByte:
                    protocol.ReadSByte();
                    return;

                case ThriftTypeId.Double:
                    protocol.ReadDouble();
                    return;

                case ThriftTypeId.Int16:
                    protocol.ReadInt16();
                    return;

                case ThriftTypeId.Int32:
                    protocol.ReadInt32();
                    return;

                case ThriftTypeId.Int64:
                    protocol.ReadInt64();
                    return;

                case ThriftTypeId.Binary:
                    protocol.ReadBinary();
                    return;

                case ThriftTypeId.List:
                    var listHeader = protocol.ReadListHeader();
                    for ( int n = 0; n < listHeader.Count; n++ )
                    {
                        Skip( listHeader.ElementTypeId, protocol );
                    }
                    protocol.ReadListEnd();
                    return;

                case ThriftTypeId.Set:
                    var setHeader = protocol.ReadSetHeader();
                    for ( int n = 0; n < setHeader.Count; n++ )
                    {
                        Skip( setHeader.ElementTypeId, protocol );
                    }
                    protocol.ReadSetEnd();
                    return;

                case ThriftTypeId.Map:
                    var mapHeader = protocol.ReadMapHeader();
                    for ( int n = 0; n < mapHeader.Count; n++ )
                    {
                        Skip( mapHeader.KeyTypeId, protocol );
                        Skip( mapHeader.ValueTypeId, protocol );
                    }
                    protocol.ReadMapEnd();
                    return;

                case ThriftTypeId.Struct:
                    protocol.ReadStructHeader();
                    while ( true )
                    {
                        var fieldHeader = protocol.ReadFieldHeader();
                        if ( fieldHeader == null )
                        {
                            break;
                        }
                        Skip( fieldHeader.FieldTypeId, protocol );
                        protocol.ReadFieldEnd();
                    }
                    protocol.ReadStructEnd();
                    return;
            }
        }

        /// <summary>
        /// Creates an expression that checks whether the specified <see cref="ThriftTypeId"/>s are equal, 
        /// and throws a <see cref="ThriftSerializationException"/> if they aren't.
        /// </summary>
        private static Expression CreateTypeIdAssert( ThriftTypeId expected, Expression actual )
        {
            return Expression.IfThen(
                Expression.NotEqual(
                    Expression.Constant( expected ),
                    actual
                ),
                Expression.Throw(
                    Expression.Call(
                        typeof( ThriftSerializationException ),
                        "TypeIdMismatch",
                        EmptyTypes,
                        Expression.Constant( expected ),
                        actual
                    )
                )
            );
        }

        /// <summary>
        /// Creates a reader for the specified type, with the specified protocol.
        /// </summary>
        private static Expression CreateReader( ParameterExpression protocolParam, ThriftType thriftType )
        {
            if ( thriftType.TypeInfo.Equals( Cache.StringTypeInfo ) )
            {
                return Expression.Call( protocolParam, "ReadString", EmptyTypes );
            }

            // force the conversion
            if ( thriftType.IsEnum || thriftType.NullableType != null )
            {
                return Expression.Convert(
                    Expression.Call( protocolParam, "Read" + thriftType.Id.ToString(), EmptyTypes ),
                    thriftType.TypeInfo.AsType()
                );
            }

            // also handles nullables thanks to implicit conversions
            if ( thriftType.IsPrimitive )
            {
                return Expression.Call( protocolParam, "Read" + thriftType.Id.ToString(), EmptyTypes );
            }

            if ( thriftType.Id == ThriftTypeId.List && thriftType.CollectionTypeInfo.IsArray )
            {
                var arrayType = thriftType.CollectionTypeInfo.AsType();
                var itemType = thriftType.ElementType.TypeInfo.AsType();
                var arrayVar = Expression.Variable( arrayType );
                var headerVar = Expression.Variable( typeof( ThriftCollectionHeader ) );
                var countVar = Expression.Variable( typeof( int ) );

                var endOfLoop = Expression.Label();

                return Expression.Block(
                    arrayType,
                    new[] { arrayVar, headerVar, countVar },
                    Expression.Assign(
                        headerVar,
                        Expression.Call( protocolParam, "ReadListHeader", EmptyTypes )
                    ),
                    CreateTypeIdAssert(
                        thriftType.ElementType.Id,
                        Expression.Field( headerVar, "ElementTypeId" )
                    ),
                    Expression.Assign(
                        arrayVar,
                        Expression.NewArrayBounds(
                            itemType,
                            Expression.Field( headerVar, "Count" )
                        )
                    ),
                    Expression.Assign(
                        countVar,
                        Expression.Constant( 0 )
                    ),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.Equal(
                                countVar,
                                Expression.Field( headerVar, "Count" )
                            ),
                            Expression.Break( endOfLoop ),
                            Expression.Block(
                                Expression.Assign(
                                    Expression.ArrayAccess( arrayVar, countVar ),
                                    Expression.Convert(
                                        CreateReader( protocolParam, thriftType.ElementType ),
                                        itemType
                                    )
                                ),
                                Expression.PostIncrementAssign( countVar )
                            )
                        ),
                        endOfLoop
                    ),
                    Expression.Call( protocolParam, "ReadListEnd", EmptyTypes ),
                    // return value:
                    arrayVar
                );
            }

            if ( thriftType.Id == ThriftTypeId.List || thriftType.Id == ThriftTypeId.Set )
            {
                string readHeaderMethodName = "Read" + thriftType.Id.ToString() + "Header";
                string readEndMethodName = "Read" + thriftType.Id.ToString() + "End";

                var collectionType = KnownCollections.GetInstantiableVersion( thriftType.TypeInfo ).AsType();
                var collectionVar = Expression.Variable( collectionType );
                var headerVar = Expression.Variable( typeof( ThriftCollectionHeader ) );
                var countVar = Expression.Variable( typeof( int ) );

                var endOfLoop = Expression.Label();

                return Expression.Block(
                    collectionType,
                    new[] { collectionVar, headerVar, countVar },
                    Expression.Assign(
                        headerVar,
                        Expression.Call( protocolParam, readHeaderMethodName, EmptyTypes )
                    ),
                    CreateTypeIdAssert(
                        thriftType.ElementType.Id,
                       Expression.Field( headerVar, "ElementTypeId" )
                    ),
                    Expression.Assign(
                        collectionVar,
                        Expression.New( collectionType )
                    ),
                    Expression.Assign(
                        countVar,
                        Expression.Constant( 0 )
                    ),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.Equal(
                                countVar,
                                Expression.Field( headerVar, "Count" )
                            ),
                            Expression.Break( endOfLoop ),
                            Expression.Block(
                                Expression.Call(
                                    collectionVar,
                                    "Add",
                                    EmptyTypes,
                                    CreateReader( protocolParam, thriftType.ElementType )
                                ),
                                Expression.PostIncrementAssign( countVar )
                            )
                        ),
                        endOfLoop
                    ),
                    Expression.Call( protocolParam, readEndMethodName, EmptyTypes ),
                    // return value:
                    collectionVar
                );
            }
            if ( thriftType.Id == ThriftTypeId.Map )
            {
                var mapType = KnownCollections.GetInstantiableVersion( thriftType.TypeInfo ).AsType();
                var mapVar = Expression.Variable( mapType );
                var headerVar = Expression.Variable( typeof( ThriftMapHeader ) );
                var countVar = Expression.Variable( typeof( int ) );

                var endOfLoop = Expression.Label();

                return Expression.Block(
                    mapType,
                    new[] { mapVar, headerVar, countVar },
                    Expression.Assign(
                        mapVar,
                        Expression.New( mapType )
                    ),
                    Expression.Assign(
                        headerVar,
                        Expression.Call( protocolParam, "ReadMapHeader", EmptyTypes )
                    ),
                    CreateTypeIdAssert(
                        thriftType.KeyType.Id,
                        Expression.Field( headerVar, "KeyTypeId" )
                    ),
                    CreateTypeIdAssert(
                        thriftType.ValueType.Id,
                        Expression.Field( headerVar, "ValueTypeId" )
                    ),
                    Expression.Assign(
                        countVar,
                        Expression.Constant( 0 )
                    ),
                    Expression.Loop(
                        Expression.IfThenElse(
                            Expression.Equal(
                                countVar,
                                Expression.Field(
                                    headerVar,
                                    "Count"
                                )
                            ),
                            Expression.Break( endOfLoop ),
                            Expression.Block(
                                Expression.Call(
                                    mapVar,
                                    "Add",
                                    EmptyTypes,
                                    CreateReader( protocolParam, thriftType.KeyType ),
                                    CreateReader( protocolParam, thriftType.ValueType )
                                ),
                                Expression.PostIncrementAssign( countVar )
                            )
                        ),
                        endOfLoop
                    ),
                    Expression.Call( protocolParam, "ReadMapEnd", EmptyTypes ),
                    // return value:
                    mapVar
                );
            }

            return Expression.Convert(
                Expression.Call(
                    typeof( ThriftReader ),
                    "Read",
                    EmptyTypes,
                    Expression.Constant( thriftType.Struct ), protocolParam, Expression.Constant( true )
                ),
                thriftType.TypeInfo.AsType()
            );
        }

        /// <summary>
        /// Creates a compiled reader for the specified struct.
        /// </summary>
        private static Func<IThriftProtocol, object> CreateCompiledReader( ThriftStruct thriftStruct )
        {
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ) );

            var structType = thriftStruct.TypeInfo.AsType();
            var structVar = Expression.Variable( structType );
            var fieldHeaderVar = Expression.Variable( typeof( ThriftFieldHeader ) );
            var setFieldsVar = Expression.Variable( typeof( HashSet<short> ) );

            var endOfLoop = Expression.Label();

            var body = Expression.Block(
                structType,
                new[] { structVar, fieldHeaderVar, setFieldsVar },
                Expression.Assign(
                    structVar,
                    Expression.New( structType )
                ),
                Expression.Assign(
                    setFieldsVar,
                    Expression.New( typeof( HashSet<short> ) )
                ),
                // ignore the return value, it's useless
                Expression.Call( protocolParam, "ReadStructHeader", EmptyTypes ),
                Expression.Loop(
                    Expression.Block(
                        Expression.Assign(
                            fieldHeaderVar,
                            Expression.Call( protocolParam, "ReadFieldHeader", EmptyTypes )
                        ),
                        Expression.IfThen(
                            Expression.Equal(
                                fieldHeaderVar,
                                Expression.Constant( null )
                            ),
                            Expression.Break( endOfLoop )
                        ),
                        thriftStruct.Fields.Any() ?
                            (Expression) Expression.Switch(
                                Expression.Field( fieldHeaderVar, "Id" ),
                                Expression.Call(
                                    typeof( ThriftReader ),
                                    "Skip",
                                    EmptyTypes,
                                    Expression.Field( fieldHeaderVar, "FieldTypeId" ), protocolParam
                                ),
                                thriftStruct.Fields.Select( f =>
                                    Expression.SwitchCase(
                                        Expression.Block(
                                            CreateTypeIdAssert(
                                                f.Header.FieldTypeId,
                                                Expression.Field( fieldHeaderVar, "FieldTypeId" )
                                            ),
                                            Expression.Call(
                                                Expression.Constant( f ),
                                                "SetValue",
                                                EmptyTypes,
                                        // need to convert 2nd param because boxing isn't automatically done
                                                structVar, Expression.Convert(
                                                    CreateReader( protocolParam, f.Header.FieldType ),
                                                    typeof( object )
                                                )
                                            ),
                                            Expression.Call(
                                                setFieldsVar,
                                                "Add",
                                                EmptyTypes,
                                                Expression.Constant( f.Header.Id )
                                            ),
                                        // void return value
                                            Expression.Empty()
                                        ),
                                        Expression.Constant( f.Header.Id )
                                    )
                                ).ToArray()
                            )
                        : Expression.Call(
                              typeof( ThriftReader ),
                              "Skip",
                              EmptyTypes,
                              Expression.Field( fieldHeaderVar, "FieldTypeId" ), protocolParam
                        ),
                        Expression.Call( protocolParam, "ReadFieldEnd", EmptyTypes )
                    ),
                    endOfLoop
                ),
                Expression.Call( protocolParam, "ReadStructEnd", EmptyTypes ),
                // now check for required fields & default values
                thriftStruct.Fields.Any() ?
                    (Expression) Expression.Block(
                        thriftStruct.Fields.Select( f =>
                            f.IsRequired ?
                                (Expression) Expression.IfThen(
                                    Expression.IsFalse(
                                        Expression.Call(
                                            setFieldsVar,
                                            "Contains",
                                            EmptyTypes,
                                            Expression.Constant( f.Header.Id )
                                        )
                                    ),
                                    Expression.Throw(
                                        Expression.Call(
                                            typeof( ThriftSerializationException ),
                                            "MissingRequiredField",
                                            EmptyTypes,
                                            Expression.Constant( thriftStruct.Header.Name ),
                                            Expression.Constant( f.Header.Name )
                                        )
                                    )
                                )
                            : f.DefaultValue.HasValue ?
                               (Expression) Expression.IfThen(
                                    Expression.IsFalse(
                                        Expression.Call(
                                            setFieldsVar,
                                            "Contains",
                                            EmptyTypes,
                                            Expression.Constant( f.Header.Id )
                                        )
                                    ),
                                    Expression.Call(
                                        Expression.Constant( f ),
                                        "SetValue",
                                        EmptyTypes,
                                // same as previous SetValue call
                                        structVar, Expression.Convert(
                                            Expression.Constant( f.DefaultValue.Value ),
                                            typeof( object )
                                        )
                                    )
                                )
                            : Expression.Empty()
                        )
                    )
                : Expression.Empty(),
                // return value:
                structVar
            );

            return Expression.Lambda<Func<IThriftProtocol, object>>( body, protocolParam ).Compile();
        }

        /// <summary>
        /// Reads the specified struct from the specified protocol.
        /// </summary>
        public static object Read( ThriftStruct thriftStruct, IThriftProtocol protocol, bool cache )
        {
            if ( !cache )
            {
                return CreateCompiledReader( thriftStruct )( protocol );
            }

            if ( !_knownReaders.ContainsKey( thriftStruct ) )
            {
                _knownReaders.Add( thriftStruct, CreateCompiledReader( thriftStruct ) );
            }

            return _knownReaders[thriftStruct]( protocol );
        }
    }
}