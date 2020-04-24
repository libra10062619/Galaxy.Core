using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Galaxy.Core
{
    public static class Awaiters
    {
        public static DetachSynchronizationContextAwaiter DetachCurrentSyncContext()
        {
            return new DetachSynchronizationContextAwaiter();
        }
    }

    public struct DetachSynchronizationContextAwaiter : ICriticalNotifyCompletion
    {
        /// <summary>
        /// Returns true if a current synchronization context is null.
        /// It means that the continuation is called only when a current context
        /// is presented.
        /// </summary>
        public bool IsCompleted => SynchronizationContext.Current == null;

        public void OnCompleted(Action continuation)
        {
            ThreadPool.QueueUserWorkItem(state => continuation());
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            ThreadPool.UnsafeQueueUserWorkItem(state => continuation(), null);
        }

        public void GetResult() { }

        public DetachSynchronizationContextAwaiter GetAwaiter() => this;
    }
}
