using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ThriftSharp.Internals;
using ThriftSharp.Protocols;

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
        private readonly IThriftProtocol _protocol;

        /// <summary>
        /// Initializes a new instance of the ThriftServiceImplementation class with the specified protocol.
        /// </summary>
        /// <param name="protocol">The protocol to call methods.</param>
        protected ThriftServiceImplementation( IThriftProtocol protocol )
        {
            _service = ThriftAttributesParser.ParseService( typeof( TService ) );
            _protocol = protocol;
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
            return CastTask<T>( Thrift.SendMessage( _protocol, GetMethod( methodName ), args ) );
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
            return CastTask<TReturn>( Thrift.SendMessage( _protocol, GetMethod( expr ), arg ) );
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
            return CastTask<TReturn>( Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2 ) );
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
            return CastTask<TReturn>( Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2, arg3 ) );
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
            return CastTask<TReturn>( Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2, arg3, arg4 ) );
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
            return (Task) Thrift.SendMessage( _protocol, GetMethod( methodName ), args );
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
            return (Task) Thrift.SendMessage( _protocol, GetMethod( expr ), arg );
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
            return (Task) Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2 );
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
            return (Task) Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2, arg3 );
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
            return (Task) Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2, arg3, arg4 );
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
            EnsureNotTask<T>();
            return (T) Thrift.SendMessage( _protocol, GetMethod( methodName ), args );
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
            EnsureNotTask<TReturn>();
            return (TReturn) Thrift.SendMessage( _protocol, GetMethod( expr ), arg );
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
            EnsureNotTask<TReturn>();
            return (TReturn) Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2 );
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
            EnsureNotTask<TReturn>();
            return (TReturn) Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2, arg3 );
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
            EnsureNotTask<TReturn>();
            return (TReturn) Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2, arg3, arg4 );
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
            Thrift.SendMessage( _protocol, GetMethod( methodName ), args );
        }

        /// <summary>
        /// Calls the specified method, with the specified argument.
        /// </summary>
        /// <typeparam name="T">The type of the method's only argument.</typeparam>
        /// <param name="expr">An expression representing the method.</param>
        /// <param name="arg">The method's only argument.</param>
        protected void Call<T>( Expression<Func<TService, Action<T>>> expr, T arg )
        {
            Thrift.SendMessage( _protocol, GetMethod( expr ), arg );
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
            Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2 );
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
            Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2, arg3 );
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
            Thrift.SendMessage( _protocol, GetMethod( expr ), arg1, arg2, arg3, arg4 );
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
        /// Gets the service's method with the specified name.
        /// </summary>
        private ThriftMethod GetMethod( string name )
        {
            return _service.Methods.First( m => m.UnderlyingName == name );
        }

        /// <summary>
        /// Gets the service's method from the specified expression.
        /// </summary>
        private ThriftMethod GetMethod<T>( Expression<T> expr )
        {
            string name = GetMethodName( expr );
            return GetMethod( name );
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