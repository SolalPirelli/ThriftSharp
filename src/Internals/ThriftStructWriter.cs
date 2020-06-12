// Copyright (c) Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using ThriftSharp.Models;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    /// <summary>
    /// Writes Thrift structs.
    /// </summary>
    internal static class ThriftStructWriter
    {
        private static readonly ConcurrentDictionary<ThriftStruct, object> _knownWriters = new ConcurrentDictionary<ThriftStruct, object>();


        /// <summary>
        /// Creates an expression writing the specified map type.
        /// </summary>
        private static Expression CreateWriterForMap( ParameterExpression protocolParam, ThriftType thriftType, Expression value )
        {
            // This code does not use IEnumerable.GetEnumerator, in order to use the "better" enumerator on collections
            // that implement their own, e.g. List<T> has a struct-returning GetEnumerator(), and a ref-returning IEnumerable<T>.GetEnumerator() 
            var getEnumeratorMethod = thriftType.TypeInfo.AsType().GetRuntimeMethod( "GetEnumerator", Types.None );

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
                        // Do not use IsTrue, not supported by UWP's expression interpreter
                        Expression.Equal(
                            Expression.Call( enumeratorVar, "MoveNext", Types.None ),
                            Expression.Constant( true )
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
        /// Creates an expression writing the specified array type.
        /// </summary>
        private static Expression CreateWriterForArray( ParameterExpression protocolParam, ThriftType thriftType, Expression value )
        {
            var counterVar = Expression.Variable( typeof( int ) );

            var endOfLoop = Expression.Label();

            return Expression.Block(
                new[] { counterVar },

                Expression.Call(
                    protocolParam,
                    Methods.IThriftProtocol_WriteListHeader,
                    Expression.New(
                        Constructors.ThriftCollectionHeader,
                          Expression.Property( value, "Length" ),
                          Expression.Constant( thriftType.ElementType.Id )
                    )
                ),

                Expression.Assign(
                    counterVar,
                    Expression.Constant( 0 )
                ),

                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(
                            counterVar,
                            Expression.Property( value, "Length" )
                        ),
                        Expression.Block(
                            CreateWriterForType(
                                protocolParam, thriftType.ElementType,
                                Expression.ArrayAccess( value, counterVar )
                            ),
                            Expression.PostIncrementAssign( counterVar )
                        ),
                        Expression.Break( endOfLoop )
                    ),
                    endOfLoop
                ),

                Expression.Call(
                    protocolParam,
                    Methods.IThriftProtocol_WriteListEnd
                )
           );
        }

        /// <summary>
        /// Creates an expression writing the specified list or set type.
        /// </summary>
        private static Expression CreateWriterForListOrSet( ParameterExpression protocolParam, ThriftType thriftType, Expression value )
        {
            // same remark as in CreateWriterForMap
            var getEnumeratorMethod = thriftType.TypeInfo.AsType().GetRuntimeMethod( "GetEnumerator", Types.None );

            var enumeratorVar = Expression.Variable( getEnumeratorMethod.ReturnType );

            var endOfLoop = Expression.Label();

            return Expression.Block(
                new[] { enumeratorVar },

                Expression.Call(
                    protocolParam,
                    thriftType.Id == ThriftTypeId.List ? Methods.IThriftProtocol_WriteListHeader : Methods.IThriftProtocol_WriteSetHeader,
                    Expression.New(
                        Constructors.ThriftCollectionHeader,
                          Expression.Property( value, "Count" ),
                          Expression.Constant( thriftType.ElementType.Id )
                    )
                ),

                Expression.Assign(
                    enumeratorVar,
                    Expression.Call( value, getEnumeratorMethod )
                ),

                Expression.Loop(
                    Expression.IfThenElse(
                        // Do not use IsTrue, not supported by UWP's expression interpreter
                        Expression.Equal(
                            Expression.Call( enumeratorVar, "MoveNext", Types.None ),
                            Expression.Constant( true )
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
        /// Creates an expression writing the specified type.
        /// </summary>
        private static Expression CreateWriterForType( ParameterExpression protocolParam, ThriftType thriftType, Expression value )
        {
            switch( thriftType.Id )
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
                    if( thriftType.IsEnum )
                    {
                        value = Expression.Convert( value, typeof( int ) );
                    }
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteInt32, value );

                case ThriftTypeId.Int64:
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteInt64, value );

                case ThriftTypeId.Binary:
                    if( thriftType.TypeInfo == TypeInfos.String )
                    {
                        return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteString, value );
                    }
                    return Expression.Call( protocolParam, Methods.IThriftProtocol_WriteBinary, value );

                case ThriftTypeId.Map:
                    return CreateWriterForMap( protocolParam, thriftType, value );

                case ThriftTypeId.Set:
                case ThriftTypeId.List:
                    if( thriftType.TypeInfo.IsArray )
                    {
                        return CreateWriterForArray( protocolParam, thriftType, value );
                    }
                    return CreateWriterForListOrSet( protocolParam, thriftType, value );

                default:
                    return Expression.Call(
                        typeof( ThriftStructWriter ),
                        "Write",
                        new[] { value.Type },
                        value, protocolParam
                    );
            }
        }

        /// <summary>
        /// Creates an expression writing the specified struct type.
        /// </summary>
        private static LambdaExpression CreateWriterForStruct( ThriftStruct thriftStruct )
        {
            var valueParam = Expression.Parameter( thriftStruct.TypeInfo.AsType() );
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ) );

            var methodContents = new List<Expression>
            {
                Expression.Call(
                    protocolParam,
                    Methods.IThriftProtocol_WriteStructHeader,
                    Expression.New(
                        Constructors.ThriftStructHeader,
                        Expression.Constant( thriftStruct.Header.Name )
                    )
                )
            };

            foreach( var field in thriftStruct.Fields )
            {
                methodContents.Add( CreateWriterForField( protocolParam, ThriftWireField.Field( field, valueParam ) ) );
            }

            methodContents.Add( Expression.Call( protocolParam, Methods.IThriftProtocol_WriteFieldStop ) );
            methodContents.Add( Expression.Call( protocolParam, Methods.IThriftProtocol_WriteStructEnd ) );

            return Expression.Lambda(
                Expression.Block( methodContents ),
                valueParam, protocolParam
            );
        }


        /// <summary>
        /// Creates an expression writing the specified field.
        /// </summary>
        public static Expression CreateWriterForField( ParameterExpression protocolParam, ThriftWireField field )
        {
            Expression getter = field.Getter;
            bool isUnderlyingNullable = Nullable.GetUnderlyingType( field.UnderlyingTypeInfo.AsType() ) != null;
            if( isUnderlyingNullable )
            {
                getter = Expression.Property( getter, "Value" );
            }
            if( field.Converter != null )
            {
                getter = field.Converter.CreateCall( "ConvertBack", getter );
            }

            var writingExpr = Expression.Block(
                Expression.Call(
                    protocolParam,
                    Methods.IThriftProtocol_WriteFieldHeader,
                    Expression.New(
                        Constructors.ThriftFieldHeader,
                        Expression.Constant( field.Id ),
                        Expression.Constant( field.Name ),
                        Expression.Constant( field.WireType.Id )
                    )
                ),
                CreateWriterForType(
                    protocolParam,
                    field.WireType,
                    getter
                ),
                Expression.Call( protocolParam, Methods.IThriftProtocol_WriteFieldEnd )
            );

            switch( field.Kind )
            {
                case ThriftWireFieldState.AlwaysPresent:
                    return writingExpr;

                case ThriftWireFieldState.Required:
                    // It's possible to have a nullable required field if it's converted to a class
                    if( field.UnderlyingTypeInfo.IsClass || isUnderlyingNullable )
                    {
                        return Expression.IfThenElse(
                            field.NullChecker,
                            Expression.Throw(
                                Expression.Call(
                                    Methods.ThriftSerializationException_RequiredFieldIsNull,
                                    Expression.Constant( field.Name )
                                )
                            ),
                            writingExpr
                        );
                    }
                    return writingExpr;

                default:
                    if( field.DefaultValue == null && ( field.UnderlyingTypeInfo.IsClass || isUnderlyingNullable ) )
                    {
                        return Expression.IfThen(
                            // Do not use IsFalse, not supported by UWP's expression interpreter
                            Expression.Equal(
                                field.NullChecker,
                                Expression.Constant( false )
                            ),
                            writingExpr
                        );
                    }

                    var condition = Expression.NotEqual(
                        getter,
                        Expression.Constant( field.DefaultValue )
                    );

                    if( field.UnderlyingTypeInfo.IsClass || isUnderlyingNullable )
                    {
                        condition = Expression.AndAlso(
                            // Do not use IsTrue, not supported by UWP's expression interpreter
                            Expression.Equal(
                                field.NullChecker,
                                Expression.Constant( false )
                            ),
                            condition
                        );
                    }

                    return Expression.IfThen( condition, writingExpr );
            }
        }


        /// <summary>
        /// Writes the specified value to the specified protocol.
        /// </summary>
        /// <remarks>
        /// This method is only called from compiled expressions.
        /// </remarks>
        public static void Write<T>( T value, IThriftProtocol protocol )
        {
            var thriftStruct = ThriftAttributesParser.ParseStruct( typeof( T ).GetTypeInfo() );
            _knownWriters.TryAdd( thriftStruct, CreateWriterForStruct( thriftStruct ).Compile() );

            ( (Action<T, IThriftProtocol>) _knownWriters[thriftStruct] )( value, protocol );
        }
    }
}