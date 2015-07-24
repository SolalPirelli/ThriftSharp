// Copyright (c) 2014-15 Solal Pirelli
// This code is licensed under the MIT License (see Licence.txt for details).

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ThriftSharp.Utilities
{
    /// <summary>
    /// Task utility methods.
    /// </summary>
    /// <remarks>
    /// The naming of this class is not optimal, but a class named TaskExtensions already exists in S.T.Tasks.
    /// </remarks>
    internal static class TaskEx
    {
        // The following two methods are adapted from http://blogs.msdn.com/b/pfxteam/archive/2011/11/10/10235834.aspx

        /// <summary>
        /// Makes the <see cref="Task{TResult}" /> timeout after the specified amount of time.
        /// </summary>
        public static Task<TResult> TimeoutAfter<TResult>( this Task<TResult> task, TimeSpan timeout )
        {
            if ( task.IsCompleted || timeout == Timeout.InfiniteTimeSpan )
            {
                return task;
            }

            var resultSource = new TaskCompletionSource<TResult>();
            var timeoutSource = new CancellationTokenSource();

            Task.Delay( timeout, timeoutSource.Token )
                .ContinueWith( ( _, state ) => ( (TaskCompletionSource<TResult>) state ).TrySetCanceled(), resultSource );

            task.ContinueWith( ( antecedent, state ) =>
                {
                    var tuple = (Tuple<CancellationTokenSource, TaskCompletionSource<TResult>>) state;
                    MarshalTaskResults( antecedent, tuple.Item2 );
                    tuple.Item1.Cancel();
                },
                Tuple.Create( timeoutSource, resultSource )
            );

            return resultSource.Task;
        }

        /// <summary>
        /// Marshals the results of the specified task to the specified task source.
        /// </summary>
        private static void MarshalTaskResults<TResult>( Task<TResult> source, TaskCompletionSource<TResult> proxy )
        {
            switch ( source.Status )
            {
                case TaskStatus.Faulted:
                    proxy.TrySetException( source.Exception.InnerException );
                    break;

                case TaskStatus.Canceled:
                    proxy.TrySetCanceled();
                    break;

                case TaskStatus.RanToCompletion:
                    proxy.TrySetResult( source.Result );
                    break;
            }
        }
    }
}