// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections.Generic;
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
        private static Expression ForType( ParameterExpression protocolParam, ThriftType thriftType )
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
                                        ForType( protocolParam, thriftType.ElementType ),
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
                                    ForType( protocolParam, thriftType.ElementType )
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
                                    ForType( protocolParam, thriftType.KeyType ),
                                    ForType( protocolParam, thriftType.ValueType )
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
                    typeof( ThriftStructReader ),
                    "Read",
                    EmptyTypes,
                    Expression.Constant( thriftType.Struct ), protocolParam
                ),
                thriftType.TypeInfo.AsType()
            );
        }

        /// <summary>
        /// Creates a compiled reader for the specified struct.
        /// </summary>
        private static Func<IThriftProtocol, object> ForStruct( ThriftStruct thriftStruct )
        {
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ) );

            var structType = thriftStruct.TypeInfo.AsType();
            var structVar = Expression.Variable( structType );

            var fieldsAndSetters = new List<Tuple<ThriftField, Func<Expression, Expression>>>();
            foreach ( var field in thriftStruct.Fields )
            {
                fieldsAndSetters.Add( Tuple.Create(
                    field,
                    (Func<Expression, Expression>) ( expr => Expression.Assign(
                        Expression.Property(
                            structVar,
                            field.BackingProperty
                        ),
                        expr
                    ) )
                ) );
            }

            var endOfLoop = Expression.Label();

            var body = Expression.Block(
                structType,
                new[] { structVar },

                Expression.Assign(
                    structVar,
                    Expression.New( structType )
                ),

                ForFields( protocolParam, fieldsAndSetters ),

                structVar // return value
            );

            return Expression.Lambda<Func<IThriftProtocol, object>>( body, protocolParam ).Compile();
        }


        /// <summary>
        /// Creates an expression reading the specified fields, given with their setter expressions, from the specified protocol.
        /// </summary>
        public static Expression ForFields( ParameterExpression protocolParam, List<Tuple<ThriftField, Func<Expression, Expression>>> fieldsAndSetters )
        {
            var fieldHeaderVar = Expression.Variable( typeof( ThriftFieldHeader ) );
            var setFieldsVar = Expression.Variable( typeof( HashSet<short> ) );

            var endOfLoop = Expression.Label();

            var fieldCases = new List<SwitchCase>();
            foreach ( var tup in fieldsAndSetters )
            {
                var setter = tup.Item2;
                if ( tup.Item1.Converter != null )
                {
                    setter = expr => tup.Item2(
                        Expression.Convert(
                            Expression.Call(
                                Expression.Constant( tup.Item1.Converter ),
                                "Convert",
                                EmptyTypes,
                                Expression.Convert(
                                    expr,
                                    typeof( object )
                                )
                            ),
                            tup.Item1.TypeInfo.AsType()
                        )
                    );
                }

                var ttype = ThriftType.Get( tup.Item1.WireTypeInfo.AsType() );

                fieldCases.Add(
                    Expression.SwitchCase(
                        Expression.Block(
                            CreateTypeIdAssert(
                                ttype.Id,
                                Expression.Field( fieldHeaderVar, "TypeId" )
                            ),
                            setter(
                                ForType( protocolParam, ttype )
                            ),
                            Expression.Call(
                                setFieldsVar,
                                "Add",
                                EmptyTypes,
                                Expression.Constant( tup.Item1.Id )
                            ),
                            Expression.Empty()  // void return value
                        ),
                        Expression.Constant( tup.Item1.Id )
                    )
                );
            }

            var skipper = Expression.Call(
                typeof( ThriftStructReader ),
                "Skip",
                EmptyTypes,
                Expression.Field( fieldHeaderVar, "TypeId" ),
                protocolParam
            );

            Expression fieldAssignment;
            if ( fieldCases.Count > 0 )
            {
                fieldAssignment = Expression.Switch(
                    Expression.Field( fieldHeaderVar, "Id" ),
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
                                Expression.Field(
                                    fieldHeaderVar,
                                    "TypeId"
                                ),
                                Expression.Constant( ThriftTypeId.Empty )
                            ),
                            Expression.Break( endOfLoop )
                        ),
                        fieldAssignment,
                        Expression.Call( protocolParam, "ReadFieldEnd", EmptyTypes )
                    ),
                    endOfLoop
                ),
                
                Expression.Call( protocolParam, "ReadStructEnd", EmptyTypes ),
            };

            // now check for required fields & default values
            foreach ( var tup in fieldsAndSetters )
            {
                if ( tup.Item1.IsRequired )
                {
                    statements.Add(
                        Expression.IfThen(
                            Expression.IsFalse(
                                Expression.Call(
                                    setFieldsVar,
                                    "Contains",
                                    EmptyTypes,
                                    Expression.Constant( tup.Item1.Id )
                                )
                            ),
                            Expression.Throw(
                                Expression.Call(
                                    typeof( ThriftSerializationException ),
                                    "MissingRequiredField",
                                    EmptyTypes,
                                    Expression.Constant( tup.Item1.Name )
                                )
                            )
                        )
                    );
                }
                else if ( tup.Item1.DefaultValue != null )
                {
                    statements.Add(
                        Expression.IfThen(
                            Expression.IsFalse(
                                Expression.Call(
                                    setFieldsVar,
                                    "Contains",
                                    EmptyTypes,
                                    Expression.Constant( tup.Item1.Id )
                                )
                            ),
                            tup.Item2(
                                Expression.Convert(
                                    Expression.Constant( tup.Item1.DefaultValue ),
                                    tup.Item1.TypeInfo.AsType()
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
        /// Reads the specified struct from the specified protocol.
        /// </summary>
        public static object Read( ThriftStruct thriftStruct, IThriftProtocol protocol )
        {
            if ( !_knownReaders.ContainsKey( thriftStruct ) )
            {
                _knownReaders.Add( thriftStruct, ForStruct( thriftStruct ) );
            }

            return _knownReaders[thriftStruct]( protocol );
        }
    }
}