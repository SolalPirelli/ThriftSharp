// Copyright (c) 2013 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ThriftSharp.Internals;
using ThriftSharp.Utilities;

namespace ThriftSharp
{
    /// <summary>
    /// Helper base class to define a Thrift service implementation over a protocol.
    /// </summary>
    /// <typeparam name="TService">The type of the service.</typeparam>
    public abstract class ThriftServiceImplementation<TService>
        where TService : class
    {
        private readonly ThriftService _service;
        private readonly ThriftCommunication _communication;

        /// <summary>
        /// Initializes a new instance of the ThriftServiceImplementation class with the specified protocol.
        /// </summary>
        /// <param name="communication">The means of communication with the server.</param>
        protected ThriftServiceImplementation( ThriftCommunication communication )
        {
            _service = ThriftAttributesParser.ParseService( typeof( TService ) );
            _communication = communication;
        }

        /// <summary>
        /// Asynchronously calls the specified method name, with the specified arguments.
        /// </summary>
        /// <typeparam name="T">The method's asynchronous return type.</typeparam>
        /// <param name="methodName">The method's name.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        /// <remarks>
        /// Clients should prefer the overloads that take expressions, to avoid hardcoded string constants.
        /// </remarks>
        protected Task<T> CallAsync<T>( string methodName, params object[] args )
        {
            Validation.IsNeitherNullNorWhitespace( methodName, () => methodName );
            Validation.IsNotNull( args, () => args );

            return CastTask<T>( Thrift.CallMethod( _communication, _service, methodName, args ) );
        }

        /// <summary>
        /// Asynchronously calls the specified method.
        /// </summary>
        /// <typeparam name="T">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task<T> CallAsync<T>( Expression<Func<TService, Func<Task<T>>>> expr )
        {
            Validation.IsNotNull( expr, () => expr );

            return CastTask<T>( Thrift.CallMethod( _communication, _service, GetMethodName( expr ) ) );
        }

        /// <summary>
        /// Asynchronously calls the specified method, with the specified argument.
        /// </summary>
        /// <typeparam name="T">The type of the method's only argument.</typeparam>
        /// <typeparam name="TReturn">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg">The method's only argument.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task<TReturn> CallAsync<T, TReturn>( Expression<Func<TService, Func<T, Task<TReturn>>>> expr, T arg )
        {
            Validation.IsNotNull( expr, () => expr );

            return CastTask<TReturn>( Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg ) );
        }

        /// <summary>
        /// Asynchronously calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="TReturn">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task<TReturn> CallAsync<T1, T2, TReturn>( Expression<Func<TService, Func<T1, T2, Task<TReturn>>>> expr, T1 arg1, T2 arg2 )
        {
            Validation.IsNotNull( expr, () => expr );

            return CastTask<TReturn>( Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2 ) );
        }

        /// <summary>
        /// Asynchronously calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="T3">The type of the method's third argument.</typeparam>
        /// <typeparam name="TReturn">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <param name="arg3">The method's third argument.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task<TReturn> CallAsync<T1, T2, T3, TReturn>( Expression<Func<TService, Func<T1, T2, T3, Task<TReturn>>>> expr, T1 arg1, T2 arg2, T3 arg3 )
        {
            Validation.IsNotNull( expr, () => expr );

            return CastTask<TReturn>( Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3 ) );
        }

        /// <summary>
        /// Asynchronously calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="T3">The type of the method's third argument.</typeparam>
        /// <typeparam name="T4">The type of the method's fourth argument.</typeparam>
        /// <typeparam name="TReturn">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <param name="arg3">The method's third argument.</param>
        /// <param name="arg4">The method's fourth argument.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task<TReturn> CallAsync<T1, T2, T3, T4, TReturn>( Expression<Func<TService, Func<T1, T2, T3, T4, Task<TReturn>>>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4 )
        {
            Validation.IsNotNull( expr, () => expr );

            return CastTask<TReturn>( Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3, arg4 ) );
        }


        /// <summary>
        /// Asynchronously calls the specified method name, with the specified arguments.
        /// </summary>
        /// <param name="methodName">The method's name.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        /// <remarks>
        /// Clients should prefer the overloads that take expressions, to avoid hardcoded string constants.
        /// </remarks>
        protected Task CallAsync( string methodName, params object[] args )
        {
            Validation.IsNeitherNullNorWhitespace( methodName, () => methodName );
            Validation.IsNotNull( args, () => args );

            return (Task) Thrift.CallMethod( _communication, _service, methodName, args );
        }

        /// <summary>
        /// Asynchronously calls the specified method.
        /// </summary>
        /// <param name="expr">An expression representing the method.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task CallAsync( Expression<Func<TService, Func<Task>>> expr )
        {
            Validation.IsNotNull( expr, () => expr );

            return (Task) Thrift.CallMethod( _communication, _service, GetMethodName( expr ) );
        }

        /// <summary>
        /// Asynchronously calls the specified method, with the specified argument.
        /// </summary>
        /// <typeparam name="T">The type of the method's only argument.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg">The method's only argument.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task CallAsync<T>( Expression<Func<TService, Func<T, Task>>> expr, T arg )
        {
            Validation.IsNotNull( expr, () => expr );

            return (Task) Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg );
        }

        /// <summary>
        /// Asynchronously calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task CallAsync<T1, T2>( Expression<Func<TService, Func<T1, T2, Task>>> expr, T1 arg1, T2 arg2 )
        {
            Validation.IsNotNull( expr, () => expr );

            return (Task) Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2 );
        }

        /// <summary>
        /// Asynchronously calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="T3">The type of the method's third argument.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <param name="arg3">The method's third argument.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task CallAsync<T1, T2, T3>( Expression<Func<TService, Func<T1, T2, T3, Task>>> expr, T1 arg1, T2 arg2, T3 arg3 )
        {
            Validation.IsNotNull( expr, () => expr );

            return (Task) Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3 );
        }

        /// <summary>
        /// Asynchronously calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="T3">The type of the method's third argument.</typeparam>
        /// <typeparam name="T4">The type of the method's fourth argument.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <param name="arg3">The method's third argument.</param>
        /// <param name="arg4">The method's fourth argument.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task CallAsync<T1, T2, T3, T4>( Expression<Func<TService, Func<T1, T2, T3, T4, Task>>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4 )
        {
            Validation.IsNotNull( expr, () => expr );

            return (Task) Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3, arg4 );
        }


        /// <summary>
        /// Calls the specified method name, with the specified arguments.
        /// </summary>
        /// <typeparam name="T">The method's return type.</typeparam>
        /// <param name="methodName">The method's name.</param>
        /// <param name="args">The method arguments.</param>
        /// <returns>The call's return value.</returns>
        /// <remarks>
        /// Clients should prefer the overloads that take expressions, to avoid hardcoded string constants.
        /// </remarks>
        protected T Call<T>( string methodName, params object[] args )
        {
            Validation.IsNeitherNullNorWhitespace( methodName, () => methodName );
            Validation.IsNotNull( args, () => args );
            EnsureNotTask<T>();

            return (T) Thrift.CallMethod( _communication, _service, methodName, args );
        }

        /// <summary>
        /// Calls the specified method.
        /// </summary>
        /// <typeparam name="T">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <returns>The call's return value.</returns>
        protected T Call<T>( Expression<Func<TService, Func<T>>> expr )
        {
            Validation.IsNotNull( expr, () => expr );

            return (T) Thrift.CallMethod( _communication, _service, GetMethodName( expr ) );
        }

        /// <summary>
        /// Calls the specified method, with the specified argument.
        /// </summary>
        /// <typeparam name="T">The type of the method's only argument.</typeparam>
        /// <typeparam name="TReturn">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg">The method's only argument.</param>
        /// <returns>The call's return value.</returns>
        protected TReturn Call<T, TReturn>( Expression<Func<TService, Func<T, TReturn>>> expr, T arg )
        {
            Validation.IsNotNull( expr, () => expr );
            EnsureNotTask<TReturn>();

            return (TReturn) Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg );
        }

        /// <summary>
        /// Calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="TReturn">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <returns>The call's return value.</returns>
        protected TReturn Call<T1, T2, TReturn>( Expression<Func<TService, Func<T1, T2, Task<TReturn>>>> expr, T1 arg1, T2 arg2 )
        {
            Validation.IsNotNull( expr, () => expr );
            EnsureNotTask<TReturn>();

            return (TReturn) Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2 );
        }

        /// <summary>
        /// Calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="T3">The type of the method's third argument.</typeparam>
        /// <typeparam name="TReturn">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <param name="arg3">The method's third argument.</param>
        /// <returns>The call's return value.</returns>
        protected TReturn Call<T1, T2, T3, TReturn>( Expression<Func<TService, Func<T1, T2, T3, Task<TReturn>>>> expr, T1 arg1, T2 arg2, T3 arg3 )
        {
            Validation.IsNotNull( expr, () => expr );
            EnsureNotTask<TReturn>();

            return (TReturn) Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3 );
        }

        /// <summary>
        /// Calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="T3">The type of the method's third argument.</typeparam>
        /// <typeparam name="T4">The type of the method's fourth argument.</typeparam>
        /// <typeparam name="TReturn">The method's return type.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <param name="arg3">The method's third argument.</param>
        /// <param name="arg4">The method's fourth argument.</param>
        /// <returns>The call's return value.</returns>
        protected TReturn Call<T1, T2, T3, T4, TReturn>( Expression<Func<TService, Func<T1, T2, T3, T4, Task<TReturn>>>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4 )
        {
            Validation.IsNotNull( expr, () => expr );
            EnsureNotTask<TReturn>();

            return (TReturn) Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3, arg4 );
        }


        /// <summary>
        /// Calls the specified method name, with the specified arguments.
        /// </summary>
        /// <param name="methodName">The method's name.</param>
        /// <param name="args">The method arguments.</param>
        /// <remarks>
        /// Clients should prefer the overloads that take expressions, to avoid hardcoded string constants.
        /// </remarks>
        protected void Call( string methodName, params object[] args )
        {
            Validation.IsNeitherNullNorWhitespace( methodName, () => methodName );
            Validation.IsNotNull( args, () => args );

            Thrift.CallMethod( _communication, _service, methodName, args );
        }

        /// <summary>
        /// Calls the specified method.
        /// </summary>
        /// <param name="expr">An expression representing the method.</param>
        protected void Call( Expression<Func<TService, Action>> expr )
        {
            Validation.IsNotNull( expr, () => expr );

            Thrift.CallMethod( _communication, _service, GetMethodName( expr ) );
        }

        /// <summary>
        /// Calls the specified method, with the specified argument.
        /// </summary>
        /// <typeparam name="T">The type of the method's only argument.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg">The method's only argument.</param>
        protected void Call<T>( Expression<Func<TService, Action<T>>> expr, T arg )
        {
            Validation.IsNotNull( expr, () => expr );

            Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg );
        }

        /// <summary>
        /// Calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        protected void Call<T1, T2>( Expression<Func<TService, Action<T1, T2>>> expr, T1 arg1, T2 arg2 )
        {
            Validation.IsNotNull( expr, () => expr );

            Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2 );
        }

        /// <summary>
        /// Calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="T3">The type of the method's third argument.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <param name="arg3">The method's third argument.</param>
        protected void Call<T1, T2, T3>( Expression<Func<TService, Action<T1, T2, T3>>> expr, T1 arg1, T2 arg2, T3 arg3 )
        {
            Validation.IsNotNull( expr, () => expr );

            Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3 );
        }

        /// <summary>
        /// Calls the specified method, with the specified arguments.
        /// </summary>
        /// <typeparam name="T1">The type of the method's first argument.</typeparam>
        /// <typeparam name="T2">The type of the method's second argument.</typeparam>
        /// <typeparam name="T3">The type of the method's third argument.</typeparam>
        /// <typeparam name="T4">The type of the method's fourth argument.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg1">The method's first argument.</param>
        /// <param name="arg2">The method's second argument.</param>
        /// <param name="arg3">The method's third argument.</param>
        /// <param name="arg4">The method's fourth argument.</param>
        protected void Call<T1, T2, T3, T4>( Expression<Func<TService, Action<T1, T2, T3, T4>>> expr, T1 arg1, T2 arg2, T3 arg3, T4 arg4 )
        {
            Validation.IsNotNull( expr, () => expr );

            Thrift.CallMethod( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3, arg4 );
        }


        /// <summary>
        /// Ensures the specified parameter type is not a Task.
        /// </summary>
        private static void EnsureNotTask<T>()
        {
            if ( typeof( Task ).IsAssignableFrom( typeof( T ) ) )
            {
                throw new InvalidOperationException( "The Call overloads cannot be used with Task return types. Use CallAsync instead." );
            }
        }

        /// <summary>
        /// Casts the specified object to a Task of the specified type.
        /// </summary>
        private static Task<T> CastTask<T>( object obj )
        {
            return ( (Task<object>) obj ).ContinueWith( x => (T) x.Result );
        }

        /// <summary>
        /// Gets the method name from an expression returning it.
        /// </summary>
        private static string GetMethodName<T>( Expression<T> expr )
        {
            return ( (MethodInfo) ( (ConstantExpression) ( (MethodCallExpression) ( (UnaryExpression) expr.Body ).Operand ).Object ).Value ).Name;
        }
    }
}