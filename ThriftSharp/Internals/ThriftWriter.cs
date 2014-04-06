// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ThriftSharp.Protocols;
using ThriftSharp.Utilities;

namespace ThriftSharp.Internals
{
    internal static class ThriftWriter
    {
        private static class Cache
        {
            public static ConstructorInfo CollectionHeaderConstructor =
                typeof( ThriftCollectionHeader ).GetTypeInfo().DeclaredConstructors.First();

            public static ConstructorInfo MapHeaderConstructor =
                typeof( ThriftMapHeader ).GetTypeInfo().DeclaredConstructors.First();

            public static PropertyInfo FieldItem =
                typeof( ReadOnlyCollection<ThriftField> ).GetTypeInfo().GetDeclaredProperty( "Item" );

            public static MethodInfo IEnumeratorMoveNext =
                    typeof( IEnumerator ).GetTypeInfo().GetDeclaredMethod( "MoveNext" );
        }

        private static readonly Type[] EmptyTypes = new Type[0];


        private static readonly IDictionary<ThriftStruct, Action<ThriftStruct, object, IThriftProtocol>> _knownWriters
            = new Dictionary<ThriftStruct, Action<ThriftStruct, object, IThriftProtocol>>();


        private static Expression CreateWriter( ParameterExpression protocolParam, ThriftType thriftType, Expression getter )
        {
            if ( thriftType.IsPrimitive )
            {
                if ( thriftType.IsNullable )
                {
                    getter = Expression.Property( getter, "Value" );
                }
                if ( thriftType.IsEnum )
                {
                    getter = Expression.Convert( getter, typeof( int ) );
                }

                string methodName = "Write" + ( thriftType.TypeInfo.AsType() == typeof( string ) ? "String" : thriftType.Id.ToString() );
                return Expression.Call( protocolParam, methodName, EmptyTypes, getter );
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
                            Expression.Property( getter, countPropertyName ),
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
                        Expression.Call( getter, enumerableTypeInfo.GetDeclaredMethod( "GetEnumerator" ) )
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
                var enumeratorType = enumerableTypeInfo.GetDeclaredMethod( "GetEnumerator" )
                                                      .ReturnType;
                var writeHeaderExpr =
                    Expression.Call(
                        protocolParam, "WriteMapHeader", EmptyTypes,
                        Expression.New(
                           Cache.MapHeaderConstructor,
                           Expression.Property( getter, "Count" ),
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
                        Expression.Call( getter, enumerableTypeInfo.GetDeclaredMethod( "GetEnumerator" ) )
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

            return Expression.Call( typeof( ThriftWriter ),
                                    "WriteValue",
                                    EmptyTypes,
                                    getter, protocolParam
                   );
        }

        private static Action<ThriftStruct, object, IThriftProtocol> CreateCompiledWriter( ThriftStruct thriftStruct )
        {
            var structParam = Expression.Parameter( typeof( ThriftStruct ), "struct" );
            var valueParam = Expression.Parameter( typeof( object ), "value" );
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ), "param" );

            var methodContents = new List<Expression>();

            methodContents.Add(
                Expression.Call( protocolParam,
                                 "WriteStructHeader", EmptyTypes,
                                 Expression.Constant( thriftStruct.Header )
                )
            );

            for ( int n = 0; n < thriftStruct.Fields.Count; n++ )
            {
                var field = thriftStruct.Fields[n];
                var fieldExpr = Expression.MakeIndex( Expression.Field( structParam, "Fields" ),
                                                      Cache.FieldItem,
                                                      new[] { Expression.Constant( n ) } );
                var fieldType = field.Header.FieldType;
                var getFieldExpr =
                    Expression.Convert(
                        Expression.Call( fieldExpr, "GetValue", EmptyTypes, valueParam ),
                        fieldType.TypeInfo.AsType()
                    );

                var writingExpr = Expression.Block(
                    Expression.Call( protocolParam,
                                     "WriteFieldHeader",
                                     EmptyTypes,
                                     Expression.Constant( field.Header )
                    ),
                    CreateWriter( protocolParam, fieldType,
                                      getFieldExpr
                    ),
                    Expression.Call( protocolParam, "WriteFieldEnd", EmptyTypes )
                );

                if ( field.DefaultValue.HasValue || fieldType.IsNullable || !fieldType.IsPrimitive )
                {
                    var defaultExpr = Expression.Constant( field.DefaultValue.HasValue ? field.DefaultValue.Value : null );
                    var isDefaultExpr = Expression.Equal( defaultExpr, getFieldExpr );

                    methodContents.Add( Expression.IfThenElse( isDefaultExpr, Expression.Empty(), writingExpr ) );
                }
                else if ( field.IsRequired && ( fieldType.IsNullable || !fieldType.IsPrimitive ) )
                {
                    var isDefaultExpr =
                        Expression.Equal(
                            Expression.Constant( null ),
                            getFieldExpr
                        );

                    var exceptionExpr =
                        Expression.Throw(
                            Expression.Call( typeof( ThriftSerializationException ),
                                             "CannotWriteNull", EmptyTypes,
                                             Expression.Constant( thriftStruct.Header.Name ),
                                             Expression.Constant( field.Header.Name )
                            )
                        );

                    methodContents.Add( Expression.IfThenElse( isDefaultExpr, exceptionExpr, writingExpr ) );
                }
                else
                {
                    methodContents.Add( writingExpr );
                }
            }

            methodContents.Add(
                Expression.Call( protocolParam, "WriteFieldStop", EmptyTypes )
            );

            methodContents.Add(
                Expression.Call( protocolParam, "WriteStructEnd", EmptyTypes )
            );

            var methodBlock = Expression.Block( methodContents );

            return (Action<ThriftStruct, object, IThriftProtocol>)
                Expression.Lambda<Action<ThriftStruct, object, IThriftProtocol>>(
                    methodBlock, structParam, valueParam, protocolParam
                ).Compile();
        }

        private static void WriteValue( object value, IThriftProtocol protocol )
        {
            var thriftStruct = ThriftAttributesParser.ParseStruct( value.GetType().GetTypeInfo() );
            Write( thriftStruct, value, protocol );
        }

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