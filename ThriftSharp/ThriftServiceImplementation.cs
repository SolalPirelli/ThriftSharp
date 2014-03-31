// Copyright (c) 2014 Solal Pirelli
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
            _service = ThriftAttributesParser.ParseService( typeof( TService ).GetTypeInfo() );
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

            return Thrift.CallMethodAsync<T>( _communication, _service, methodName, args );
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

            return Thrift.CallMethodAsync<T>( _communication, _service, GetMethodName( expr ) );
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

            return Thrift.CallMethodAsync<TReturn>( _communication, _service, GetMethodName( expr ), arg );
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

            return Thrift.CallMethodAsync<TReturn>( _communication, _service, GetMethodName( expr ), arg1, arg2 );
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

            return Thrift.CallMethodAsync<TReturn>( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3 );
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

            return Thrift.CallMethodAsync<TReturn>( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3, arg4 );
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

            return Thrift.CallMethodAsync<object>( _communication, _service, methodName, args );
        }

        /// <summary>
        /// Asynchronously calls the specified method.
        /// </summary>
        /// <param name="expr">An expression representing the method.</param>
        /// <returns>The task object representing the asynchronous call.</returns>
        protected Task CallAsync( Expression<Func<TService, Func<Task>>> expr )
        {
            Validation.IsNotNull( expr, () => expr );

            return Thrift.CallMethodAsync<object>( _communication, _service, GetMethodName( expr ) );
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

            return Thrift.CallMethodAsync<object>( _communication, _service, GetMethodName( expr ), arg );
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

            return Thrift.CallMethodAsync<object>( _communication, _service, GetMethodName( expr ), arg1, arg2 );
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

            return Thrift.CallMethodAsync<object>( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3 );
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

            return Thrift.CallMethodAsync<object>( _communication, _service, GetMethodName( expr ), arg1, arg2, arg3, arg4 );
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