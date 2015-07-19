// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Reads Thrift structs.
    /// </summary>
    internal static class ThriftStructReader
    {
        private static readonly Dictionary<ThriftStruct, Func<IThriftProtocol, object>> _knownReaders
            = new Dictionary<ThriftStruct, Func<IThriftProtocol, object>>();


        /// <summary>
        /// Creates an expression that checks whether the specified IDs are equal, and throws if they aren't.
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
                        Methods.ThriftSerializationException_TypeIdMismatch,
                        Expression.Constant( expected ), actual
                    )
                )
            );
        }

        /// <summary>
        /// Creates an expression reading the specified map type.
        /// </summary>
        private static Expression CreateReaderForMap( ParameterExpression protocolParam, ThriftType thriftType )
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
                    Expression.Call( protocolParam, Methods.IThriftProtocol_ReadMapHeader )
                ),

                CreateTypeIdAssert(
                    thriftType.KeyType.Id,
                    Expression.Field( headerVar, Fields.ThriftMapHeader_KeyTypeId )
                ),

                CreateTypeIdAssert(
                    thriftType.ValueType.Id,
                    Expression.Field( headerVar, Fields.ThriftMapHeader_ValueTypeId )
                ),

                Expression.Assign(
                    countVar,
                    Expression.Constant( 0 )
                ),

                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(
                            countVar,
                            Expression.Field( headerVar, Fields.ThriftMapHeader_Count )
                        ),
                        Expression.Break( endOfLoop ),
                        Expression.Block(
                            Expression.Call(
                                mapVar,
                                "Add", // TODO make knowncollections prettier
                                Types.None,
                                CreateReaderForType( protocolParam, thriftType.KeyType ),
                                CreateReaderForType( protocolParam, thriftType.ValueType )
                            ),
                            Expression.PostIncrementAssign( countVar )
                        )
                    ),
                    endOfLoop
                ),

                Expression.Call( protocolParam, Methods.IThriftProtocol_ReadMapEnd ),

                // return value
                mapVar
            );
        }

        /// <summary>
        /// Creates an expression reading the specified array type.
        /// </summary>
        private static Expression CreateReaderForArray( ParameterExpression protocolParam, ThriftType thriftType )
        {
            var arrayType = thriftType.CollectionTypeInfo.AsType();
            var itemType = thriftType.ElementType.TypeInfo.AsType();
            var arrayVar = Expression.Variable( arrayType );
            var headerVar = Expression.Variable( typeof( ThriftCollectionHeader ) );
            var lengthVar = Expression.Variable( typeof( int ) );

            var endOfLoop = Expression.Label();

            return Expression.Block(
                arrayType,
                new[] { arrayVar, headerVar, lengthVar },

                Expression.Assign(
                    headerVar,
                    Expression.Call( protocolParam, Methods.IThriftProtocol_ReadListHeader )
                ),

                CreateTypeIdAssert(
                    thriftType.ElementType.Id,
                    Expression.Field( headerVar, Fields.ThriftCollectionHeader_ElementTypeId )
                ),

                Expression.Assign(
                    arrayVar,
                    Expression.NewArrayBounds(
                        itemType,
                        Expression.Field( headerVar, Fields.ThriftCollectionHeader_Count )
                    )
                ),

                Expression.Assign(
                    lengthVar,
                    Expression.Constant( 0 )
                ),

                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Equal(
                            lengthVar,
                            Expression.Field( headerVar, Fields.ThriftCollectionHeader_Count )
                        ),
                        Expression.Break( endOfLoop ),
                        Expression.Block(
                            Expression.Assign(
                                Expression.ArrayAccess(
                                    arrayVar,
                                    lengthVar
                                ),
                                CreateReaderForType( protocolParam, thriftType.ElementType )
                            ),
                            Expression.PostIncrementAssign( lengthVar )
                        )
                    ),
                    endOfLoop
                ),

                Expression.Call( protocolParam, Methods.IThriftProtocol_ReadListEnd ),

                // return value
                arrayVar
            );
        }

        /// <summary>
        /// Creates an expression reading the specified list or set type.
        /// </summary>
        private static Expression CreateReaderForListOrSet( ParameterExpression protocolParam, ThriftType thriftType )
        {
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
                    Expression.Call(
                        protocolParam,
                        thriftType.Id == ThriftTypeId.List ? Methods.IThriftProtocol_ReadListHeader : Methods.IThriftProtocol_ReadSetHeader
                    )
                ),

                CreateTypeIdAssert(
                    thriftType.ElementType.Id,
                   Expression.Field( headerVar, Fields.ThriftCollectionHeader_ElementTypeId )
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
                            Expression.Field( headerVar, Fields.ThriftCollectionHeader_Count )
                        ),
                        Expression.Break( endOfLoop ),
                        Expression.Block(
                            Expression.Call(
                                collectionVar,
                                "Add", // TODO make knowncollections prettier
                                Types.None,
                                CreateReaderForType( protocolParam, thriftType.ElementType )
                            ),
                            Expression.PostIncrementAssign( countVar )
                        )
                    ),
                    endOfLoop
                ),

                Expression.Call(
                    protocolParam,
                    thriftType.Id == ThriftTypeId.List ? Methods.IThriftProtocol_ReadListEnd : Methods.IThriftProtocol_ReadSetEnd
                ),

                // return value
                collectionVar
            );
        }

        /// <summary>
        /// Creates a reader for the specified type.
        /// </summary>
        private static Expression CreateReaderForType( ParameterExpression protocolParam, ThriftType thriftType )
        {
            if ( thriftType.NullableType != null )
            {
                return Expression.Convert(
                    CreateReaderForType( protocolParam, thriftType.NullableType ),
                    thriftType.TypeInfo.AsType()
                );
            }

            switch ( thriftType.Id )
            {
                case ThriftTypeId.Boolean:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_ReadBoolean );

                case ThriftTypeId.SByte:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_ReadSByte );

                case ThriftTypeId.Double:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_ReadDouble );

                case ThriftTypeId.Int16:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_ReadInt16 );

                case ThriftTypeId.Int32:
                    if ( thriftType.TypeInfo.IsEnum )
                    {
                        return Expression.Convert(
                            Expression.Call( protocolParam, Methods.IThriftProtocol_ReadInt32 ),
                            thriftType.TypeInfo.AsType()
                        );
                    }
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_ReadInt32 );

                case ThriftTypeId.Int64:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_ReadInt64 );

                case ThriftTypeId.Binary:
                    if ( thriftType.TypeInfo == TypeInfos.String )
                    {
                        return Expression.Call( protocolParam, Methods.IThriftProtocol_ReadString );
                    }
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_ReadBinary );

                case ThriftTypeId.Struct:
                    return Expression.Convert(
                        Expression.Call(
                            Methods.ThriftStructReader_Read,
                            Expression.Constant( thriftType.Struct ), protocolParam
                        ),
                        thriftType.TypeInfo.AsType()
                    );

                case ThriftTypeId.Map:
                    return CreateReaderForMap( protocolParam, thriftType );

                case ThriftTypeId.Set:
                case ThriftTypeId.List:
                    if ( thriftType.CollectionTypeInfo.IsArray )
                    {
                        return CreateReaderForArray( protocolParam, thriftType );
                    }
                    return CreateReaderForListOrSet( protocolParam, thriftType );

                default:
                    throw new InvalidOperationException( "Cannot create a reader for the type " + thriftType.Id );
            }
        }

        /// <summary>
        /// Creates a reader for the specified struct.
        /// </summary>
        private static Expression<Func<IThriftProtocol, object>> CreateReaderForStruct( ThriftStruct thriftStruct )
        {
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ) );

            var structType = thriftStruct.TypeInfo.AsType();
            var structVar = Expression.Variable( structType );

            var fieldsAndSetters = new Dictionary<ThriftField, Func<Expression, Expression>>();
            foreach ( var field in thriftStruct.Fields )
            {
                fieldsAndSetters.Add(
                    field,
                    expr => Expression.Assign(
                        Expression.Property(
                            structVar,
                            field.BackingProperty
                        ),
                        expr
                    )
                );
            }

            var endOfLoop = Expression.Label();

            var body = Expression.Block(
                structType,
                new[] { structVar },

                Expression.Assign(
                    structVar,
                    Expression.New( structType )
                ),

                CreateReaderForFields( protocolParam, fieldsAndSetters ),

                // return value
                structVar
            );

            return Expression.Lambda<Func<IThriftProtocol, object>>( body, protocolParam );
        }


        /// <summary>
        /// Creates an expression reading the specified fields, given with their setter expressions.
        /// </summary>
        public static Expression CreateReaderForFields( ParameterExpression protocolParam, Dictionary<ThriftField, Func<Expression, Expression>> fieldsAndSetters )
        {
            var fieldHeaderVar = Expression.Variable( typeof( ThriftFieldHeader ) );
            var setFieldsVar = Expression.Variable( typeof( HashSet<short> ) );

            var endOfLoop = Expression.Label();

            var fieldCases = new List<SwitchCase>();
            foreach ( var pair in fieldsAndSetters )
            {
                var setter = pair.Value;
                if ( pair.Key.Converter != null )
                {
                    if ( pair.Key.Type.NullableType == null )
                    {
                        setter = expr => pair.Value(
                            Expression.Call(
                                Expression.Constant( pair.Key.Converter ),
                                "Convert", // The converter type is unknown here (even the interface since it's generic)
                                Types.None,
                                expr
                            )
                        );
                    }
                    else
                    {
                        setter = expr => pair.Value(
                            Expression.Convert(
                                Expression.Call(
                                    Expression.Constant( pair.Key.Converter ),
                                    "Convert", // idem
                                    Types.None,
                                    Expression.Convert(
                                        expr,
                                        pair.Key.Type.NullableType.TypeInfo.AsType()
                                    )
                                ),
                                pair.Key.UnderlyingTypeInfo.AsType()
                            )
                        );
                    }
                }

                fieldCases.Add(
                    Expression.SwitchCase(
                        Expression.Block(
                            CreateTypeIdAssert(
                                pair.Key.Type.Id,
                                Expression.Field( fieldHeaderVar, Fields.ThriftFieldHeader_TypeId )
                            ),
                            setter(
                                CreateReaderForType( protocolParam, pair.Key.Type )
                            ),
                            Expression.Call(
                                setFieldsVar,
                                Methods.HashSetOfShort_Add,
                                Expression.Constant( pair.Key.Id )
                            ),
                    // Need to "return" void here because the default case value does it (skipping fields)
                            Expression.Empty()
                        ),
                        Expression.Constant( pair.Key.Id )
                    )
                );
            }

            var skipper = Expression.Call(
                Methods.ThriftStructReader_Skip,
                Expression.Field( fieldHeaderVar, Fields.ThriftFieldHeader_TypeId ), protocolParam
            );

            Expression fieldAssignment;
            if ( fieldCases.Count > 0 )
            {
                // Switch doesn't support 0 cases
                fieldAssignment = Expression.Switch(
                    Expression.Field( fieldHeaderVar, Fields.ThriftFieldHeader_Id ),
                    skipper,
                    fieldCases.ToArray()
                );
            }
            else
            {
                fieldAssignment = skipper;
            }


            var statements = new List<Expression>
            {
                Expression.Assign(
                    setFieldsVar,
                    Expression.New( setFieldsVar.Type )
                ),
                
                // ignore the return value, it's useless
                Expression.Call( protocolParam, Methods.IThriftProtocol_ReadStructHeader ),

                Expression.Loop(
                    Expression.Block(
                        Expression.Assign(
                            fieldHeaderVar,
                            Expression.Call( protocolParam, Methods.IThriftProtocol_ReadFieldHeader )
                        ),
                        Expression.IfThen(
                            Expression.Equal(
                                Expression.Field( fieldHeaderVar, Fields.ThriftFieldHeader_TypeId ),
                                Expression.Constant( ThriftTypeId.Empty )
                            ),
                            Expression.Break( endOfLoop )
                        ),
                        fieldAssignment,
                        Expression.Call( protocolParam, Methods.IThriftProtocol_ReadFieldEnd )
                    ),
                    endOfLoop
                ),
                
                Expression.Call( protocolParam, Methods.IThriftProtocol_ReadStructEnd ),
            };

            // now check for required fields & default values
            foreach ( var pair in fieldsAndSetters )
            {
                if ( pair.Key.IsRequired )
                {
                    statements.Add(
                        Expression.IfThen(
                            Expression.IsFalse(
                                Expression.Call(
                                    setFieldsVar,
                                    Methods.HashSetOfShort_Contains,
                                    Expression.Constant( pair.Key.Id )
                                )
                            ),
                            Expression.Throw(
                                Expression.Call(
                                    Methods.ThriftSerializationException_MissingRequiredField,
                                    Expression.Constant( pair.Key.Name )
                                )
                            )
                        )
                    );
                }
                else if ( pair.Key.DefaultValue != null )
                {
                    statements.Add(
                        Expression.IfThen(
                            Expression.IsFalse(
                                Expression.Call(
                                    setFieldsVar,
                                    Methods.HashSetOfShort_Contains,
                                    Expression.Constant( pair.Key.Id )
                                )
                            ),
                            pair.Value(
                                Expression.Convert(
                                    Expression.Constant( pair.Key.DefaultValue ),
                                    pair.Key.UnderlyingTypeInfo.AsType()
                                )
                            )
                        )
                    );
                }
            }

            return Expression.Block(
                new[] { fieldHeaderVar, setFieldsVar },
                statements
            );
        }


        /// <summary>
        /// Skips the specified ID from the specified protocol.
        /// </summary>
        /// <remarks>
        /// This method is only called from generated expressions.
        /// </remarks>
        public static void Skip( ThriftTypeId thriftTypeId, IThriftProtocol protocol )
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
                        if ( fieldHeader.TypeId == ThriftTypeId.Empty )
                        {
                            break;
                        }
                        Skip( fieldHeader.TypeId, protocol );
                        protocol.ReadFieldEnd();
                    }
                    protocol.ReadStructEnd();
                    return;
            }
        }

        /// <summary>
        /// Reads the specified struct from the specified protocol.
        /// </summary>
        /// <remarks>
        /// This method is only called from generated expressions.
        /// </remarks>
        public static object Read( ThriftStruct thriftStruct, IThriftProtocol protocol )
        {
            if ( !_knownReaders.ContainsKey( thriftStruct ) )
            {
                _knownReaders.Add( thriftStruct, CreateReaderForStruct( thriftStruct ).Compile() );
            }

            return _knownReaders[thriftStruct]( protocol );
        }
    }
}