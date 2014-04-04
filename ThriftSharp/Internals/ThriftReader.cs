// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ThriftSharp.Protocols;

namespace ThriftSharp.Internals
{
    internal static class ThriftReader
    {
        private static readonly MethodInfo NonGenericTaskContinueWith = TypeInfos.Task.GetDeclaredMethods( "ContinueWith" )
            .First( m => m.IsGenericMethodDefinition && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof( Func<,> ) );

        // TODO during struct parsing ensure it's one of these or a type with a ctor and no args
        private static readonly IDictionary<Type, Type> KnownImplementations = new Dictionary<Type, Type>
        {
            { typeof( ISet<> ), typeof( HashSet<> ) },
            { typeof( ICollection<> ), typeof( List<> ) },
            { typeof( IDictionary<,> ), typeof( Dictionary<,> ) }
        };

        private static readonly IDictionary<ThriftStruct, Func<ThriftStruct, IThriftProtocol, Task<object>>> _knownReaders
            = new Dictionary<ThriftStruct, Func<ThriftStruct, IThriftProtocol, Task<object>>>();

        private static TypeInfo GetCollectionTypeInfo( TypeInfo typeInfo )
        {
            if ( typeInfo.IsInterface )
            {
                return KnownImplementations[typeInfo.MakeGenericType()].MakeGenericType( typeInfo.GenericTypeArguments ).GetTypeInfo();
            }
            return typeInfo;
        }

        private static Expression ContinueTaskExpressionWith( Expression task, LambdaExpression continuation )
        {
            var taskParam = Expression.Parameter( typeof( Task ), "task" );
            return Expression.Call( task, NonGenericTaskContinueWith,
                Expression.Lambda(
                    Expression.Switch( Expression.Property( taskParam, TypeInfos.Task.GetDeclaredProperty( "Status" ) ),
                        Expression.SwitchCase(
                            Expression.Call( continuation.Compile().GetMethodInfo() ),
                            Expression.Constant( TaskStatus.RanToCompletion ) ),
                        Expression.SwitchCase(
                            Expression.Throw(
                                Expression.Property(
                                    Expression.Property( taskParam, TypeInfos.Task.GetDeclaredProperty( "Exception" ) ),
                                    TypeInfos.AggregateException.GetDeclaredMethod( "InnerException" ) ) ),
                            Expression.Constant( TaskStatus.Faulted ) ),
                        Expression.SwitchCase(
                            Expression.Throw(
                                Expression.New( TypeInfos.TaskCanceledException.DeclaredConstructors.First( c => c.GetParameters().Length == 0 ) ) ),
                            Expression.Constant( TaskStatus.Canceled ) ) ),
                    taskParam ) );
        }

        private static Expression ContinueGenericTaskExpressionWith( Expression task, Type parameterType, LambdaExpression continuation )
        {
            var taskType = typeof( Task<> ).MakeGenericType( parameterType );
            var taskParam = Expression.Parameter( taskType, "task" );
            return Expression.Call( task, NonGenericTaskContinueWith,
                Expression.Lambda(
                    Expression.Switch( Expression.Property( taskParam, TypeInfos.Task.GetDeclaredProperty( "Status" ) ),
                        Expression.SwitchCase(
                            Expression.Call( continuation.Compile().GetMethodInfo(), Expression.Property( taskParam, taskType.GetTypeInfo().GetDeclaredProperty( "Result" ) ) ),
                            Expression.Constant( TaskStatus.RanToCompletion ) ),
                        Expression.SwitchCase(
                            Expression.Throw(
                                Expression.Property(
                                    Expression.Property( taskParam, TypeInfos.Task.GetDeclaredProperty( "Exception" ) ),
                                    TypeInfos.AggregateException.GetDeclaredMethod( "InnerException" ) ) ),
                            Expression.Constant( TaskStatus.Faulted ) ),
                        Expression.SwitchCase(
                            Expression.Throw(
                                Expression.New( TypeInfos.TaskCanceledException.DeclaredConstructors.First( c => c.GetParameters().Length == 0 ) ) ),
                            Expression.Constant( TaskStatus.Canceled ) ) ),
                    taskParam ) );
        }


        private static Expression CreateAsyncReader( ParameterExpression protocolParam, ThriftType thriftType )
        {
            if ( thriftType.IsPrimitive )
            {
                return Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "Read" + thriftType.TypeInfo.Name + "Async" ) );
            }

            if ( thriftType.CollectionTypeInfo != null )
            {
                string readHeader, readEnd;
                if ( thriftType.IsSet )
                {
                    readHeader = "ReadSetHeaderAsync";
                    readEnd = "ReadSetEndAsync";
                }
                else
                {
                    readHeader = "ReadListHeaderAsync";
                    readEnd = "ReadListEndAsync";
                }

                bool isArray = thriftType.CollectionTypeInfo.IsArray;
                var collectionTypeInfo = isArray ? thriftType.CollectionTypeInfo : GetCollectionTypeInfo( thriftType.CollectionTypeInfo );

                var headerParam = Expression.Parameter( typeof( ThriftCollectionHeader ), "header" );
                var elementParam = Expression.Parameter( thriftType.ElementType.TypeInfo.AsType(), "element" );
                var counterVar = Expression.Variable( typeof( int ), "counter" );
                var collectionVar = Expression.Variable( collectionTypeInfo.AsType(), "collection" );
                var loopLabel = Expression.Label();
                var endLabel = Expression.Label();
                // TODO check that the expected and actual element types match
                return ContinueGenericTaskExpressionWith(
                    Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( readHeader ) ),
                    typeof( ThriftCollectionHeader ),
                    Expression.Lambda<Func<ThriftCollectionHeader>>(
                        Expression.Block( new[] { counterVar, collectionVar },
                            Expression.Assign( counterVar, Expression.Constant( -1 ) ),
                            Expression.Assign( collectionVar,
                                isArray ? Expression.NewArrayBounds( collectionTypeInfo.AsType(), Expression.Property( headerParam, TypeInfos.CollectionHeader.GetDeclaredProperty( "Count" ) ) )
                                        : (Expression) Expression.New( collectionTypeInfo.DeclaredConstructors.First( c => c.GetParameters().Length == 0 ) ) ),
                            Expression.Label( loopLabel ),
                            Expression.Increment( counterVar ),
                            Expression.IfThen( Expression.Equal(
                                                   counterVar,
                                                   Expression.Add(
                                                       Expression.Property( headerParam, TypeInfos.CollectionHeader.GetDeclaredProperty( "Count" ) ),
                                                       Expression.Constant( -1 )
                                                   )
                                               ),
                                               Expression.Goto( endLabel ) ),
                            ContinueGenericTaskExpressionWith(
                                CreateAsyncReader( protocolParam, thriftType.ElementType ),
                                thriftType.ElementType.TypeInfo.AsType(),
                                Expression.Lambda(
                                    Expression.Block(
                                        isArray ? Expression.Assign( Expression.ArrayAccess( collectionVar, counterVar ), elementParam )
                                                : (Expression) Expression.Call( collectionVar, collectionTypeInfo.GetDeclaredMethod( "Add" ), elementParam ),
                                        Expression.Goto( loopLabel )
                                    ),
                                    elementParam
                                )
                            ),
                            Expression.Label( endLabel ),
                            Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( readEnd ) )
                        ),
                        headerParam ) );
            }

            if ( thriftType.MapTypeInfo != null )
            {
                var collectionTypeInfo = GetCollectionTypeInfo( thriftType.MapTypeInfo );

                var headerParam = Expression.Parameter( typeof( ThriftMapHeader ), "header" );
                var keyParam = Expression.Parameter( thriftType.KeyType.TypeInfo.AsType(), "key" );
                var valueParam = Expression.Parameter( thriftType.ValueType.TypeInfo.AsType(), "value" );
                var counterVar = Expression.Variable( typeof( int ), "counter" );
                var collectionVar = Expression.Variable( collectionTypeInfo.AsType(), "collection" );
                var loopLabel = Expression.Label();
                var endLabel = Expression.Label();
                // TODO check that the expected and actual key/value types match
                return ContinueGenericTaskExpressionWith(
                    Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "ReadMapHeaderAsync" ) ),
                    typeof( ThriftCollectionHeader ),
                    Expression.Lambda<Func<ThriftCollectionHeader>>(
                        Expression.Block( new[] { counterVar, collectionVar },
                            Expression.Assign( counterVar, Expression.Constant( 0 ) ),
                            Expression.Assign( collectionVar, Expression.New( collectionTypeInfo.DeclaredConstructors.First( c => c.GetParameters().Length == 0 ) ) ),
                            Expression.Label( loopLabel ),
                            Expression.Increment( counterVar ),
                            Expression.IfThen( Expression.Equal( counterVar, Expression.Property( headerParam, TypeInfos.MapHeader.GetDeclaredProperty( "Count" ) ) ),
                                               Expression.Goto( endLabel ) ),
                            ContinueGenericTaskExpressionWith(
                                CreateAsyncReader( protocolParam, thriftType.KeyType ),
                                thriftType.KeyType.TypeInfo.AsType(),
                                Expression.Lambda(
                                    ContinueGenericTaskExpressionWith(
                                        CreateAsyncReader( protocolParam, thriftType.ValueType ),
                                        thriftType.ValueType.TypeInfo.AsType(),
                                        Expression.Lambda(
                                            Expression.Block(
                                                Expression.Call( collectionVar, collectionTypeInfo.GetDeclaredMethod( "Add" ), keyParam, valueParam ),
                                                Expression.Goto( loopLabel )
                                            ),
                                            valueParam
                                        )
                                    ),
                                    keyParam
                                )
                            ),
                            Expression.Label( endLabel ),
                            Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "ReadMapEndAsync" ) )
                        ),
                        headerParam ) );
            }

            return Expression.Call( TypeInfos.Reader.GetDeclaredMethod( "ReadAsync" ),
                                        Expression.Constant( thriftType.StructType ), protocolParam
                );
        }

        private static Func<ThriftStruct, IThriftProtocol, Task<object>> CreateCompiledAsyncReader( ThriftStruct thriftStruct )
        {
            var structParam = Expression.Parameter( typeof( ThriftStruct ), "thriftStruct" );
            var protocolParam = Expression.Parameter( typeof( IThriftProtocol ), "protocol" );

            var structHeaderParam = Expression.Parameter( typeof( ThriftStructHeader ), "structHeader" );
            var fieldHeaderParam = Expression.Parameter( typeof( ThriftFieldHeader ), "fieldHeader" );

            var objVar = Expression.Variable( thriftStruct.TypeInfo.AsType(), "obj" );

            var loopLabel = Expression.Label();
            var endLabel = Expression.Label();

            return Expression.Lambda<Func<ThriftStruct, IThriftProtocol, Task<object>>>(
                Expression.Block(
                    new[] { objVar },
                    Expression.Assign(
                        objVar,
                        Expression.New( thriftStruct.TypeInfo.DeclaredConstructors.First( c => c.GetParameters().Length == 0 ) )
                    ),
                    ContinueGenericTaskExpressionWith(
                        Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "ReadStructHeaderAsync" ) ),
                        typeof( ThriftStructHeader ),
                        Expression.Lambda(
                            Expression.Block(
                                Expression.Label( loopLabel ),
                                ContinueGenericTaskExpressionWith(
                                    Expression.Call( protocolParam, TypeInfos.Protocol.GetDeclaredMethod( "ReadFieldHeaderAsync" ) ),
                                    typeof( ThriftFieldHeader ),
                                    Expression.Lambda(
                                        Expression.Block(
                                            Expression.IfThen(
                                                Expression.Equal(
                                                    fieldHeaderParam,
                                                    Expression.Constant( null )
                                                ),
                                                Expression.Goto( endLabel )
                                            ),
                                            Expression.Switch(
                                                Expression.Property( fieldHeaderParam, TypeInfos.FieldHeader.GetDeclaredProperty( "Id" ) ),
                                                ( from pair in thriftStruct.Fields.Select( ( f, i ) => new { Field = f, Index = i } )
                                                  let fieldParam = Expression.Parameter( pair.Field.Header.FieldType.TypeInfo.AsType(), "fieldValue" )
                                                  select
                                                     Expression.SwitchCase(
                                                         Expression.Block(
                                                             ContinueGenericTaskExpressionWith(
                                                                 CreateAsyncReader( protocolParam, pair.Field.Header.FieldType ),
                                                                 pair.Field.Header.FieldType.TypeInfo.AsType(),
                                                                 Expression.Lambda(
                                                                     Expression.Call(
                                                                         Expression.MakeIndex(
                                                                             Expression.Field( structParam, TypeInfos.Struct.GetDeclaredField( "Fields" ) ),
                                                                             TypeInfos.FieldCollection.GetDeclaredProperty( "Item" ),
                                                                              new[] { Expression.Constant( pair.Index ) }
                                                                         ),
                                                                         TypeInfos.Field.GetDeclaredMethod( "SetValue" ),
                                                                         objVar,
                                                                         fieldParam
                                                                     )
                                                                 )
                                                             ),
                                                             Expression.Goto( loopLabel )
                                                         ),
                                                         Expression.Constant( pair.Field.Header.Id )
                                                     )
                                                ).ToArray()
                                            )
                                        ),
                                        fieldHeaderParam
                                    )
                                )
                            ),
                            structHeaderParam
                        )
                    )
                )
            ).Compile();
        }


        public static Task<object> ReadAsync( ThriftStruct thriftStruct, IThriftProtocol protocol )
        {
            if ( !_knownReaders.ContainsKey( thriftStruct ) )
            {
                _knownReaders.Add( thriftStruct, null );
            }

            return _knownReaders[thriftStruct]( thriftStruct, protocol );
        }
    }
}