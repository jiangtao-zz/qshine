using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace qshine.Utility
{
    /// <summary>
    /// Help to run async method as Sync
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _taskFactory = new
            TaskFactory(CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        /// <summary>
        /// Wrap an async method in sync call
        /// </summary>
        /// <typeparam name="TResult">Type of the result from async task</typeparam>
        /// <param name="func">async method call</param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

        /// <summary>
        /// Wrap an async function in sync call
        /// </summary>
        /// <param name="func">async function call</param>
        public static void RunSync(Func<Task> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
    }
}
