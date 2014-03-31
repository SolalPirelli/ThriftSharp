// Copyright (c) 2014 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).
// Redistributions of this source code must retain the above copyright notice.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Task utility methods.
    /// </summary>
    public static class TaskEx
    {
        /// <summary>
        /// Creates a Task from the old .NET async model, with a timeout.
        /// </summary>
        public static Task<TResult> FromAsync<TResult>( Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, int millisecondsTimeout )
        {
            return Task.Factory.FromAsync( beginMethod, res => endMethod( res ), null ).TimeoutAfter( millisecondsTimeout );
        }

        /// <summary>
        /// Returns a Task that times out after the specified timeout, 
        /// or executes the current task if it completes before the timeout occurs.
        /// </summary>
        public static Task<TResult> TimeoutAfter<TResult>( this Task<TResult> task, int millisecondsTimeout )
        {
            var tokenSource = new CancellationTokenSource();
            return Task.WhenAny( task, Task.Delay( millisecondsTimeout, tokenSource.Token )
                                           .ContinueWith( _ => default( TResult ) ) )
                       .ContinueWith( ( t, s ) =>
                       {
                           var source = (CancellationTokenSource) s;
                           if ( source.IsCancellationRequested )
                           {
                               throw new TimeoutException();
                           }

                           source.Cancel();

                           switch ( t.Result.Status )
                           {
                               case TaskStatus.Faulted:
                                   throw t.Result.Exception;

                               case TaskStatus.Canceled:
                                   throw new TimeoutException();

                               case TaskStatus.RanToCompletion:
                                   return t.Result.Result;

                               default:
                                   throw new InvalidOperationException( "This should never happen." );
                           }
                       }, tokenSource );
        }
    }
}
