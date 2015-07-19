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
    /// Writes Thrift structs.
    /// </summary>
    internal static class ThriftStructWriter
    {
        private static readonly Dictionary<ThriftStruct, Action<object, IThriftProtocol>> _knownWriters
            = new Dictionary<ThriftStruct, Action<object, IThriftProtocol>>();


        /// <summary>
        /// Creates an expression writing the specified map type.
        /// </summary>
        private static Expression CreateWriterForMap( ParameterExpression protocolParam, ThriftType thriftType, Expression value )
        {
            var enumerableTypeInfo = thriftType.MapTypeInfo
                                                  .GetGenericInterface( typeof( IEnumerable<> ) )
                                                  .GetTypeInfo();
            var getEnumeratorMethod = enumerableTypeInfo.GetDeclaredMethod( "GetEnumerator" );


            var endOfLoop = Expression.Label();
            var enumeratorVar = Expression.Variable( getEnumeratorMethod.ReturnType );

            return Expression.Block(
                new[] { enumeratorVar },

                Expression.Call(
                    protocolParam,
                    Methods.IThriftProtocol_WriteMapHeader,
                    Expression.New(
                       Constructors.ThriftMapHeader,
                       Expression.Property( value, "Count" ),
                       Expression.Constant( thriftType.KeyType.Id ),
                       Expression.Constant( thriftType.ValueType.Id )
                    )
                ),

                Expression.Assign(
                    enumeratorVar,
                    Expression.Call( value, getEnumeratorMethod )
                ),

                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.IsTrue(
                            Expression.Call( enumeratorVar, Methods.IEnumerator_MoveNext )
                        ),
                        Expression.Block(
                            CreateWriterForType(
                                protocolParam, thriftType.KeyType,
                                  Expression.Property(
                                      Expression.Property( enumeratorVar, "Current" ),
                                      "Key"
                                  )
                            ),
                            CreateWriterForType(
                                protocolParam, thriftType.ValueType,
                                  Expression.Property(
                                      Expression.Property( enumeratorVar, "Current" ),
                                      "Value"
                                  )
                            )
                        ),
                        Expression.Break( endOfLoop )
                    ),
                    endOfLoop
                ),

                Expression.Call( protocolParam, Methods.IThriftProtocol_WriteMapEnd )
            );
        }

        /// <summary>
        /// Creates an expression writing the specified collection type.
        /// </summary>
        private static Expression CreateWriterForCollection( ParameterExpression protocolParam, ThriftType thriftType, Expression value )
        {
            var enumerableTypeInfo = thriftType.CollectionTypeInfo
                                               .GetGenericInterface( typeof( IEnumerable<> ) )
                                               .GetTypeInfo();
            var getEnumeratorMethod = enumerableTypeInfo.GetDeclaredMethod( "GetEnumerator" );

            var enumeratorVar = Expression.Variable( getEnumeratorMethod.ReturnType );

            var endOfLoop = Expression.Label();

            return Expression.Block(
                new[] { enumeratorVar },

                Expression.Call(
                    protocolParam,
                    thriftType.Id == ThriftTypeId.List ? Methods.IThriftProtocol_WriteListHeader : Methods.IThriftProtocol_WriteSetHeader,
                    Expression.New(
                        Constructors.ThriftCollectionHeader,
                          Expression.Property( value, thriftType.CollectionTypeInfo.IsArray ? "Length" : "Count" ),
                          Expression.Constant( thriftType.ElementType.Id )
                    )
                ),

                Expression.Assign(
                    enumeratorVar,
                    Expression.Call( value, getEnumeratorMethod )
                ),

                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.IsTrue(
                            Expression.Call( enumeratorVar, Methods.IEnumerator_MoveNext )
                        ),
                        CreateWriterForType(
                            protocolParam, thriftType.ElementType,
                            Expression.Property( enumeratorVar, "Current" )
                        ),
                        Expression.Break( endOfLoop )
                    ),
                    endOfLoop
                ),

                Expression.Call(
                    protocolParam,
                    thriftType.Id == ThriftTypeId.List ? Methods.IThriftProtocol_WriteListEnd : Methods.IThriftProtocol_WriteSetEnd
                )
           );
        }

        /// <summary>
        /// Creates a writer for the specified type, with the specified protocol and value.
        /// </summary>
        private static Expression CreateWriterForType( ParameterExpression protocolParam, ThriftType thriftType, Expression value )
        {
            if ( thriftType.NullableType != null )
            {
                return CreateWriterForType( protocolParam, thriftType.NullableType, Expression.Property( value, "Value" ) );
            }

            switch ( thriftType.Id )
            {
                case ThriftTypeId.Boolean:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteBoolean, value );

                case ThriftTypeId.SByte:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteSByte, value );

                case ThriftTypeId.Double:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteDouble, value );

                case ThriftTypeId.Int16:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteInt16, value );

                case ThriftTypeId.Int32:
                    if ( thriftType.TypeInfo.IsEnum )
                    {
                        value = Expression.Convert( value, typeof( int ) );
                    }
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteInt32, value );

                case ThriftTypeId.Int64:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteInt64, value );

                case ThriftTypeId.Binary:
                    if ( thriftType.TypeInfo == TypeInfos.String )
                    {
                        return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteString, value );
                    }
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteBinary, value );

                case ThriftTypeId.Struct:
                    return Expression.Call(
                        Methods.ThriftStructWriter_Write,
                        Expression.Constant( thriftType.Struct ), value, protocolParam
                    );

                case ThriftTypeId.Map:
                    return CreateWriterForMap( protocolParam, thriftType, value );

                case ThriftTypeId.Set:
                case ThriftTypeId.List:
                    return CreateWriterForCollection( protocolParam, thriftType, value );

                default:
                    throw new InvalidOperationException( "Cannot create a writer for the type " + thriftType.Id );
            }
        }

        /// <summary>
        /// Creates a writer for the specified struct.
        /// </summary>
        private static Expression<Action<object, IThriftProtocol>> CreateWriterForStruct( ThriftStruct thriftStruct )
        {
            var valueParam = Expression.Parameter( typeof( object ) );
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ) );

            var methodContents = new List<Expression>
            {
                Expression.Call(
                    protocolParam,
                    Methods.IThriftProtocol_WriteStructHeader,
                    Expression.Constant(thriftStruct.Header)
                )
            };

            foreach ( var field in thriftStruct.Fields )
            {
                var getFieldExpr = Expression.Property(
                    Expression.Convert(
                        valueParam,
                        thriftStruct.TypeInfo.AsType()
                    ),
                    field.BackingProperty
                );

                methodContents.Add( CreateWriterForField( protocolParam, field, getFieldExpr ) );
            }

            methodContents.Add( Expression.Call( protocolParam, Methods.IThriftProtocol_WriteFieldStop ) );
            methodContents.Add( Expression.Call( protocolParam, Methods.IThriftProtocol_WriteStructEnd ) );

            return Expression.Lambda<Action<object, IThriftProtocol>>(
                Expression.Block( methodContents ),
                valueParam, protocolParam
            );
        }


        /// <summary>
        /// Creates a writer expression for the specified field with the specified getter, using the specified protocol expression.
        /// </summary>
        public static Expression CreateWriterForField( ParameterExpression protocolParam, ThriftField field, Expression getter )
        {
            if ( field.Converter != null )
            {
                if ( field.Type.NullableType == null )
                {
                    getter = Expression.Call(
                        Expression.Constant( field.Converter ),
                        "ConvertBack",
                        Types.None,
                        getter
                    );
                }
                else
                {
                    getter = Expression.Convert(
                        Expression.Call(
                            Expression.Constant( field.Converter ),
                            "ConvertBack",
                            Types.None,
                            Expression.Convert(
                                getter,
                                Nullable.GetUnderlyingType( field.UnderlyingTypeInfo.AsType() )
                            )
                        ),
                        field.WireType
                    );
                }
            }

            var writingExpr = Expression.Block(
                Expression.Call(
                    protocolParam,
                    Methods.IThriftProtocol_WriteFieldHeader,
                    Expression.New(
                        Constructors.ThriftFieldHeader,
                        Expression.Constant( field.Id ),
                        Expression.Constant( field.Name ),
                        Expression.Constant( field.Type.Id )
                    )
                ),
                CreateWriterForType(
                    protocolParam,
                    field.Type,
                    getter
                ),
                Expression.Call( protocolParam, Methods.IThriftProtocol_WriteFieldEnd )
            );


            if ( field.IsRequired && ( field.Type.NullableType != null || field.Type.TypeInfo.IsClass ) )
            {
                return Expression.IfThenElse(
                    Expression.Equal(
                        getter,
                        Expression.Constant( null )
                    ),
                    Expression.Throw(
                        Expression.Call(
                            Methods.ThriftSerializationException_RequiredFieldIsNull,
                            Expression.Constant( field.Name )
                        )
                    ),
                    writingExpr
                );
            }
            if ( field.DefaultValue != null || field.Type.NullableType != null || field.UnderlyingTypeInfo.IsClass )
            {
                Expression defaultValueExpr;
                // if it has a default value, use it
                if ( field.DefaultValue != null )
                {
                    if ( field.UnderlyingTypeInfo.IsClass )
                    {
                        // if it's a class, it's OK
                        defaultValueExpr = Expression.Constant( field.DefaultValue );
                    }
                    else
                    {
                        // otherwise we need to make the default value a Nullable.
                        defaultValueExpr = Expression.New(
                           field.UnderlyingTypeInfo.DeclaredConstructors.Single(),
                           Expression.Constant( field.DefaultValue )
                       );
                    }
                }
                else
                {
                    // otherwise it's always a reference type
                    defaultValueExpr = Expression.Constant( null );
                }

                return Expression.IfThen(
                    Expression.NotEqual(
                        getter,
                        defaultValueExpr
                    ),
                    writingExpr
                );
            }

            return writingExpr;

        }


        /// <summary>
        /// Writes the specified value (with the struct also specified) to the specified protocol.
        /// </summary>
        /// <remarks>
        /// This method is only called from compiled expressions.
        /// </remarks>
        public static void Write( ThriftStruct thriftStruct, object value, IThriftProtocol protocol )
        {
            if ( !_knownWriters.ContainsKey( thriftStruct ) )
            {
                _knownWriters.Add( thriftStruct, CreateWriterForStruct( thriftStruct ).Compile() );
            }

            _knownWriters[thriftStruct]( value, protocol );
        }
    }
}