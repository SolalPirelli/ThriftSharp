// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Collections;
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
    internal static class ThriftWriter
    {
        // Cached common values
        private static class Cache
        {
            public static readonly ConstructorInfo CollectionHeaderConstructor =
                typeof( ThriftCollectionHeader ).GetTypeInfo().DeclaredConstructors.First();

            public static readonly ConstructorInfo MapHeaderConstructor =
                typeof( ThriftMapHeader ).GetTypeInfo().DeclaredConstructors.First();

            public static readonly PropertyInfo FieldItem =
                typeof( IReadOnlyList<ThriftField> ).GetTypeInfo().GetDeclaredProperty( "Item" );

            public static readonly MethodInfo IEnumeratorMoveNext =
                typeof( IEnumerator ).GetTypeInfo().GetDeclaredMethod( "MoveNext" );
        }

        // Empty Types array, widely used in expression trees
        private static readonly Type[] EmptyTypes = new Type[0];


        // Cached compiled writers
        private static readonly Dictionary<ThriftStruct, Action<ThriftStruct, object, IThriftProtocol>> _knownWriters
            = new Dictionary<ThriftStruct, Action<ThriftStruct, object, IThriftProtocol>>();


        /// <summary>
        /// Creates a writer for the specified type, with the specified protocol and value.
        /// </summary>
        private static Expression CreateWriter( ParameterExpression protocolParam, ThriftType thriftType, Expression value )
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
                return Expression.Call( protocolParam, methodName, EmptyTypes, value );
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
                        writeHeader, EmptyTypes,
                        Expression.New(
                            Cache.CollectionHeaderConstructor,
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
                            Expression.Call( enumeratorVar, Cache.IEnumeratorMoveNext )
                        ),
                        CreateWriter(
                            protocolParam, thriftType.ElementType,
                            Expression.Property( enumeratorVar, "Current" )
                        ),
                        Expression.Break( endOfLoop )
                    ),
                    endOfLoop
                );
                var writeEndExpr = Expression.Call( protocolParam, writeEnd, EmptyTypes );

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
                        protocolParam, "WriteMapHeader", EmptyTypes,
                        Expression.New(
                           Cache.MapHeaderConstructor,
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
                            Expression.Call( enumeratorVar, Cache.IEnumeratorMoveNext )
                        ),
                        Expression.Block(
                            CreateWriter( protocolParam, thriftType.KeyType,
                                              Expression.Property( enumeratorCurrentExpr, "Key" )
                            ),
                            CreateWriter( protocolParam, thriftType.ValueType,
                                              Expression.Property( enumeratorCurrentExpr, "Value" )
                            )
                        ),
                        Expression.Break( endOfLoop )
                    ),
                    endOfLoop
                );
                var writeEndExpr = Expression.Call( protocolParam, "WriteMapEnd", EmptyTypes );

                return Expression.Block( new[] { enumeratorVar }, writeHeaderExpr, enumeratorAssign, loopExpr, writeEndExpr );
            }

            return Expression.Call(
                       typeof( ThriftWriter ),
                       "Write", EmptyTypes,
                       Expression.Constant( thriftType.Struct ),
                       value,
                       protocolParam
                   );
        }

        /// <summary>
        /// Creates a compiled writer for the specified struct.
        /// </summary>
        private static Action<ThriftStruct, object, IThriftProtocol> CreateCompiledWriter( ThriftStruct thriftStruct )
        {
            var structParam = Expression.Parameter( typeof( ThriftStruct ), "struct" );
            var valueParam = Expression.Parameter( typeof( object ), "value" );
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ), "param" );

            var methodContents = new List<Expression>
            {
                Expression.Call(
                    protocolParam,
                    "WriteStructHeader", EmptyTypes,
                    Expression.Constant(thriftStruct.Header)
                )
            };

            for ( int n = 0; n < thriftStruct.Fields.Count; n++ )
            {
                var field = thriftStruct.Fields[n];
                var fieldExpr =
                    Expression.MakeIndex(
                        Expression.Field( structParam, "Fields" ),
                        Cache.FieldItem,
                        new[] { Expression.Constant( n ) }
                    );

                var fieldType = field.Header.FieldType;
                var getFieldExpr =
                    Expression.Convert(
                        Expression.Call( fieldExpr, "GetValue", EmptyTypes, valueParam ),
                        fieldType.TypeInfo.AsType()
                    );

                var writingExpr = Expression.Block(
                    Expression.Call(
                        protocolParam,
                        "WriteFieldHeader", EmptyTypes,
                        Expression.Constant( field.Header )
                    ),
                    CreateWriter( protocolParam, fieldType, getFieldExpr ),
                    Expression.Call( protocolParam, "WriteFieldEnd", EmptyTypes )
                );


                if ( field.IsRequired && ( fieldType.NullableType != null || fieldType.TypeInfo.IsClass ) )
                {
                    var isDefaultExpr = Expression.Equal( Expression.Constant( null ), getFieldExpr );

                    var exceptionExpr =
                        Expression.Throw(
                            Expression.Call(
                                typeof( ThriftSerializationException ),
                                "RequiredFieldIsNull", EmptyTypes,
                                Expression.Constant( thriftStruct.Header.Name ),
                                Expression.Constant( field.Header.Name )
                            )
                        );

                    methodContents.Add( Expression.IfThenElse( isDefaultExpr, exceptionExpr, writingExpr ) );
                }
                else if ( field.DefaultValue.HasValue || fieldType.NullableType != null || fieldType.TypeInfo.IsClass )
                {
                    Expression defaultValueExpr;
                    // if it has a default value, use it
                    if ( field.DefaultValue.HasValue )
                    {
                        if ( fieldType.TypeInfo.IsClass )
                        {
                            // if it's a class, it's OK
                            defaultValueExpr = Expression.Constant( field.DefaultValue.Value );
                        }
                        else
                        {
                            // otherwise we need to make the default value a Nullable.
                            defaultValueExpr =
                                Expression.New(
                                   fieldType.TypeInfo // is a nullable of the right type
                                            .DeclaredConstructors
                                            .First(), // Nullable<T> only has one ctor,
                                   Expression.Constant( field.DefaultValue.Value )
                           );
                        }
                    }
                    else
                    {
                        // otherwise it's always a reference type
                        defaultValueExpr = Expression.Constant( null );
                    }

                    methodContents.Add(
                        Expression.IfThen(
                            Expression.NotEqual(
                                getFieldExpr,
                                defaultValueExpr
                            ),
                            writingExpr
                        )
                    );
                }
                else
                {
                    methodContents.Add( writingExpr );
                }
            }

            methodContents.Add( Expression.Call( protocolParam, "WriteFieldStop", EmptyTypes ) );
            methodContents.Add( Expression.Call( protocolParam, "WriteStructEnd", EmptyTypes ) );

            return Expression.Lambda<Action<ThriftStruct, object, IThriftProtocol>>(
                Expression.Block( methodContents ),
                structParam,
                valueParam,
                protocolParam
            ).Compile();
        }


        /// <summary>
        /// Writes the specified value (with the struct also specified) to the specified protocol.
        /// </summary>
        public static void Write( ThriftStruct thriftStruct, object value, IThriftProtocol protocol )
        {
            if ( !_knownWriters.ContainsKey( thriftStruct ) )
            {
                _knownWriters.Add( thriftStruct, CreateCompiledWriter( thriftStruct ) );
            }

            _knownWriters[thriftStruct]( thriftStruct, value, protocol );
        }
    }
}