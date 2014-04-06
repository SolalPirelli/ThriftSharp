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
        public static Task<TResult> FromAsync<TResult>( Func<AsyncCallback, object, IAsyncResult> beginMethod, Func<IAsyncResult, TResult> endMethod, int millisecondsTimeout, CancellationToken token )
        {
            return Task.Factory.FromAsync( beginMethod, res => endMethod( res ), null ).TimeoutAfter( millisecondsTimeout, token );
        }

        /// <summary>
        /// Returns a Task that times out after the specified timeout, 
        /// or executes the current task if it completes before the timeout occurs.
        /// </summary>
        private static Task<TResult> TimeoutAfter<TResult>( this Task<TResult> task, int millisecondsTimeout, CancellationToken token )
        {
            var timeoutSource = new CancellationTokenSource();
            var tokenSource = CancellationTokenSource.CreateLinkedTokenSource( timeoutSource.Token, token );

            var timeoutTask = Task.Delay( millisecondsTimeout, tokenSource.Token )
                                  .ContinueWith( _ => default( TResult ) );

            var continuationState = Tuple.Create( timeoutSource, timeoutTask );

            return Task.WhenAny( task, timeoutTask )
                       .ContinueWith( ( t, s ) =>
                       {
                           var state = (Tuple<CancellationTokenSource, Task<TResult>>) s;

                           if ( t.Result == state.Item2 )
                           {
                               throw new OperationCanceledException();
                           }

                           state.Item1.Cancel();
                           state.Item1.Dispose();

                           switch ( t.Result.Status )
                           {
                               case TaskStatus.Faulted:
                                   throw t.Result.Exception;

                               case TaskStatus.Canceled:
                                   throw new OperationCanceledException();

                               case TaskStatus.RanToCompletion:
                                   return t.Result.Result;

                               default:
                                   throw new InvalidOperationException( "This should never happen." );
                           }
                       }, continuationState );
        }
    }
}
