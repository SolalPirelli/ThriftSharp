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
    internal static class ThriftStructReader
    {
        private static readonly Dictionary<ThriftStruct, object> _knownReaders = new Dictionary<ThriftStruct, object>();


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
                    return Expression.Call(
                        typeof( ThriftStructReader ),
                        "Read",
                        new[] { thriftType.TypeInfo.AsType() },
                        Expression.Constant( thriftType.Struct ), protocolParam
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
        private static LambdaExpression CreateReaderForStruct( ThriftStruct thriftStruct )
        {
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ) );

            var structType = thriftStruct.TypeInfo.AsType();
            var structVar = Expression.Variable( structType );

            var wireFields = thriftStruct.Fields.Select( f => ThriftWireField.Field( f, structVar ) ).ToList();

            return Expression.Lambda(
                Expression.Block(
                    structType,
                    new[] { structVar },

                    Expression.Assign(
                        structVar,
                        Expression.New( structType )
                    ),

                    CreateReaderForFields( protocolParam, wireFields ),

                    // return value
                    structVar
                ),
                protocolParam
            );
        }


        /// <summary>
        /// Creates an expression reading the specified fields, given with their setter expressions.
        /// </summary>
        public static Expression CreateReaderForFields( ParameterExpression protocolParam, List<ThriftWireField> wireFields )
        {
            var fieldHeaderVar = Expression.Variable( typeof( ThriftFieldHeader ) );
            var isSetVars = wireFields.Where( f => f.UnderlyingType.GetTypeInfo().IsValueType && ( f.IsRequired || f.DefaultValue != null ) )
                                      .ToDictionary( f => f, _ => Expression.Variable( typeof( bool ) ) );

            var endOfLoop = Expression.Label();

            var fieldCases = new List<SwitchCase>();
            foreach ( var field in wireFields )
            {
                var setter = field.Setter;
                if ( field.Converter != null )
                {
                    if ( field.WireType.NullableType == null )
                    {
                        setter = expr => field.Setter(
                            Expression.Call(
                                Expression.Constant( field.Converter ),
                                "Convert", // The converter type is unknown here (even the interface since it's generic)
                                Types.None,
                                expr
                            )
                        );
                    }
                    else
                    {
                        setter = expr => field.Setter(
                            Expression.Convert(
                                Expression.Call(
                                    Expression.Constant( field.Converter ),
                                    "Convert", // idem
                                    Types.None,
                                    Expression.Convert(
                                        expr,
                                        field.WireType.NullableType.TypeInfo.AsType()
                                    )
                                ),
                                field.UnderlyingType
                            )
                        );
                    }
                }

                var caseStatements = new List<Expression>
                {
                    CreateTypeIdAssert(
                        field.WireType.Id,
                        Expression.Field( fieldHeaderVar, Fields.ThriftFieldHeader_TypeId )
                    ),
                    setter(
                        CreateReaderForType( protocolParam, field.WireType )
                    )
                };

                if ( isSetVars.ContainsKey( field ) )
                {
                    caseStatements.Add(
                        Expression.Assign(
                            isSetVars[field],
                            Expression.Constant( true )
                        )
                    );
                }

                // Need to return void here because the default case value does it (skipping fields)
                caseStatements.Add( Expression.Empty() );

                fieldCases.Add(
                    Expression.SwitchCase(
                        Expression.Block( caseStatements ),
                        Expression.Constant( field.Id )
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

            // now check for default values
            foreach ( var field in wireFields )
            {
                // The 'check' declaration needs to be inside this test, since the return value of a method:
                // - isn't in isSetVars
                // - isn't required (to provide the proper ThriftProtocolException instead of a generic "unset field")
                // - might be a value type
                // thus it could pick the "== null" branch and crash
                if ( field.DefaultValue != null || field.IsRequired )
                {
                    var check = isSetVars.ContainsKey( field )
                      ? (Expression) Expression.IsFalse( isSetVars[field] )
                      : Expression.Equal( field.Getter, Expression.Constant( null ) );

                    if ( field.DefaultValue != null )
                    {
                        statements.Add(
                            Expression.IfThen(
                                check,
                                field.Setter(
                                    Expression.Convert(
                                        Expression.Constant( field.DefaultValue ),
                                        field.WireType.TypeInfo.AsType()
                                    )
                                )
                            )
                        );
                    }
                    else
                    {
                        statements.Add(
                            Expression.IfThen(
                                check,
                                Expression.Throw(
                                    Expression.Call(
                                        Methods.ThriftSerializationException_MissingRequiredField,
                                        Expression.Constant( field.Name )
                                    )
                                )
                            )
                        );
                    }
                }
            }

            return Expression.Block(
                isSetVars.Values.Concat( new[] { fieldHeaderVar } ),
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
        public static T Read<T>( ThriftStruct thriftStruct, IThriftProtocol protocol )
        {
            if ( !_knownReaders.ContainsKey( thriftStruct ) )
            {
                _knownReaders.Add( thriftStruct, CreateReaderForStruct( thriftStruct ).Compile() );
            }

            return ( (Func<IThriftProtocol, T>) _knownReaders[thriftStruct] )( protocol );
        }
    }
}