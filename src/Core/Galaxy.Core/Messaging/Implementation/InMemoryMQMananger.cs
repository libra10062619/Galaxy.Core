using Galaxy.Core.Abstractions;
using System;
using System.Collections.Concurrent;

namespace Galaxy.Core.Messaging.Implementation
{
    internal class InMemoryMQMananger : DisposableObj, IDisposable
    {
        readonly static ConcurrentDictionary<string, ConcurrentQueue<IMQMessage>> inMemoryQueue
            = new ConcurrentDictionary<string, ConcurrentQueue<IMQMessage>>();

        public void AddMessage(IMQMessage message)
        {
            var key = message.GetMessageKey();
            var queue = inMemoryQueue.GetOrAdd(key, new ConcurrentQueue<IMQMessage>());
            queue.Enqueue(message);
        }

        protected override void Disposing()
        {
            inMemoryQueue.Clear();
        }
    }
}
