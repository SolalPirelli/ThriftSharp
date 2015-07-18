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
        /// Creates a writer for the specified type, with the specified protocol and value.
        /// </summary>
        private static Expression ForType( ParameterExpression protocolParam, ThriftType thriftType, Expression value )
        {
            if ( thriftType.IsPrimitive )
            {
                if ( thriftType.NullableType != null )
                {
                    value = Expression.Property( value, "Value" );
                    thriftType = thriftType.NullableType;
                }
                if ( thriftType.IsEnum )
                {
                    value = Expression.Convert( value, typeof( int ) );
                }

                string methodName = "Write" + ( thriftType.TypeInfo.AsType() == typeof( string ) ? "String" : thriftType.Id.ToString() );
                return Expression.Call( protocolParam, methodName, Types.EmptyTypes, value );
            }

            if ( thriftType.CollectionTypeInfo != null )
            {
                string writeHeader, writeEnd;
                if ( thriftType.Id == ThriftTypeId.Set )
                {
                    writeHeader = "WriteSetHeader";
                    writeEnd = "WriteSetEnd";
                }
                else
                {
                    writeHeader = "WriteListHeader";
                    writeEnd = "WriteListEnd";
                }

                string countPropertyName = thriftType.CollectionTypeInfo.IsArray ? "Length" : "Count";

                var writeHeaderExpr =
                    Expression.Call(
                        protocolParam,
                        writeHeader, Types.EmptyTypes,
                        Expression.New(
                            Constructors.ThriftCollectionHeader,
                            Expression.Property( value, countPropertyName ),
                            Expression.Constant( thriftType.ElementType.Id )
                        )
                    );

                var enumerableTypeInfo = thriftType.CollectionTypeInfo
                                                   .GetGenericInterface( typeof( IEnumerable<> ) )
                                                   .GetTypeInfo();
                var enumeratorType = enumerableTypeInfo.GetDeclaredMethod( "GetEnumerator" ).ReturnType;

                var enumeratorVar = Expression.Variable( enumeratorType, "enumerator" );
                var enumeratorAssign =
                    Expression.Assign(
                        enumeratorVar,
                    // Can't use the string-taking Call() overload because we need the generic GetEnumerator
                        Expression.Call( value, enumerableTypeInfo.GetDeclaredMethod( "GetEnumerator" ) )
                    );

                var endOfLoop = Expression.Label();
                var loopExpr = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.IsTrue(
                            Expression.Call( enumeratorVar, Methods.IEnumerator_MoveNext )
                        ),
                        ForType(
                            protocolParam, thriftType.ElementType,
                            Expression.Property( enumeratorVar, "Current" )
                        ),
                        Expression.Break( endOfLoop )
                    ),
                    endOfLoop
                );
                var writeEndExpr = Expression.Call( protocolParam, writeEnd, Types.EmptyTypes );

                return Expression.Block( new[] { enumeratorVar }, writeHeaderExpr, enumeratorAssign, loopExpr, writeEndExpr );
            }

            if ( thriftType.MapTypeInfo != null )
            {
                var enumerableTypeInfo = thriftType.MapTypeInfo
                                                  .GetGenericInterface( typeof( IEnumerable<> ) )
                                                  .GetTypeInfo();
                var enumeratorType = enumerableTypeInfo.GetDeclaredMethod( "GetEnumerator" ).ReturnType;
                var writeHeaderExpr =
                    Expression.Call(
                        protocolParam, "WriteMapHeader", Types.EmptyTypes,
                        Expression.New(
                           Constructors.ThriftMapHeader,
                           Expression.Property( value, "Count" ),
                           Expression.Constant( thriftType.KeyType.Id ),
                           Expression.Constant( thriftType.ValueType.Id )
                        )
                    );

                var endOfLoop = Expression.Label();
                var enumeratorVar = Expression.Variable( enumeratorType, "enumerator" );
                var enumeratorCurrentExpr = Expression.Property( enumeratorVar, "Current" );

                var enumeratorAssign =
                    Expression.Assign(
                        enumeratorVar,
                        Expression.Call( value, enumerableTypeInfo.GetDeclaredMethod( "GetEnumerator" ) )
                    );

                var loopExpr = Expression.Loop(
                    Expression.IfThenElse(
                        Expression.IsTrue(
                    // Can't use the string-using Call() overload because MoveNext is only declared on the non-generic IEnumerator
                            Expression.Call( enumeratorVar, Methods.IEnumerator_MoveNext )
                        ),
                        Expression.Block(
                            ForType( protocolParam, thriftType.KeyType,
                                              Expression.Property( enumeratorCurrentExpr, "Key" )
                            ),
                            ForType( protocolParam, thriftType.ValueType,
                                              Expression.Property( enumeratorCurrentExpr, "Value" )
                            )
                        ),
                        Expression.Break( endOfLoop )
                    ),
                    endOfLoop
                );
                var writeEndExpr = Expression.Call( protocolParam, "WriteMapEnd", Types.EmptyTypes );

                return Expression.Block( new[] { enumeratorVar }, writeHeaderExpr, enumeratorAssign, loopExpr, writeEndExpr );
            }

            return Expression.Call(
                typeof( ThriftStructWriter ),
                "Write",
                Types.EmptyTypes,
                Expression.Constant( thriftType.Struct ), value, protocolParam
            );
        }

        /// <summary>
        /// Creates a compiled writer for the specified struct.
        /// </summary>
        private static Action<object, IThriftProtocol> ForStruct( ThriftStruct thriftStruct )
        {
            var valueParam = Expression.Parameter( typeof( object ), "value" );
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ), "param" );

            var methodContents = new List<Expression>
            {
                Expression.Call(
                    protocolParam,
                    "WriteStructHeader", Types.EmptyTypes,
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

                methodContents.Add( ForField( protocolParam, field, getFieldExpr ) );
            }

            methodContents.Add( Expression.Call( protocolParam, "WriteFieldStop", Types.EmptyTypes ) );
            methodContents.Add( Expression.Call( protocolParam, "WriteStructEnd", Types.EmptyTypes ) );

            return Expression.Lambda<Action<object, IThriftProtocol>>(
                Expression.Block( methodContents ),
                valueParam,
                protocolParam
            ).Compile();
        }


        /// <summary>
        /// Creates a writer expression for the specified field with the specified getter, using the specified protocol expression.
        /// </summary>
        public static Expression ForField( ParameterExpression protocolParam, ThriftField field, Expression getter )
        {
            if ( field.Converter != null )
            {
                if ( field.Type.NullableType == null )
                {
                    getter = Expression.Call(
                        Expression.Constant( field.Converter ),
                        "ConvertBack",
                        Types.EmptyTypes,
                        getter
                    );
                }
                else
                {
                    getter = Expression.Convert(
                        Expression.Call(
                            Expression.Constant( field.Converter ),
                            "ConvertBack",
                            Types.EmptyTypes,
                            Expression.Convert(
                                getter,
                                Nullable.GetUnderlyingType( field.UnderlyingTypeInfo.AsType() )
                            )
                        ),
                        field.WireType
                    );
                }
            }

            var read = ForType(
                protocolParam,
                field.Type,
                getter
            );

            var writingExpr = Expression.Block(
                Expression.Call(
                    protocolParam,
                    "WriteFieldHeader",
                    Types.EmptyTypes,
                    Expression.New(
                        Constructors.ThriftFieldHeader,
                        Expression.Constant( field.Id ),
                        Expression.Constant( field.Name ),
                        Expression.Constant( field.Type.Id )
                    )
                ),
                read,
                Expression.Call( protocolParam, "WriteFieldEnd", Types.EmptyTypes )
            );


            if ( field.IsRequired && ( field.Type.NullableType != null || field.Type.TypeInfo.IsClass ) )
            {
                var isDefaultExpr = Expression.Equal( Expression.Constant( null ), getter );

                var exceptionExpr =
                    Expression.Throw(
                        Expression.Call(
                            typeof( ThriftSerializationException ),
                            "RequiredFieldIsNull",
                            Types.EmptyTypes,
                            Expression.Constant( field.Name )
                        )
                    );

                return Expression.IfThenElse( isDefaultExpr, exceptionExpr, writingExpr );
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
                        defaultValueExpr =
                            Expression.New(
                               field.UnderlyingTypeInfo  // is a nullable of the right type
                                    .DeclaredConstructors
                                    .First(), // Nullable<T> only has one ctor
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
        public static void Write( ThriftStruct thriftStruct, object value, IThriftProtocol protocol )
        {
            if ( !_knownWriters.ContainsKey( thriftStruct ) )
            {
                _knownWriters.Add( thriftStruct, ForStruct( thriftStruct ) );
            }

            _knownWriters[thriftStruct]( value, protocol );
        }
    }
}