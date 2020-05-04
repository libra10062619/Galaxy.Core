using Galaxy.Core.Abstractions;
using Galaxy.Core.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxy.Core.EventBus
{
    public interface IEventBusSubscriptionManager : IDisposable
    {
        Task AddSubscription<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>;

        //Task AddSubscription<TEvent>(Func<IEventHandler<TEvent>> activitor) where TEvent : IIntegrationEvent;

        Task RemoveSubscription<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>;

        //Task RemoveSubscription<TEvent>(Func<IEventHandler<TEvent>> activitor) where TEvent : IIntegrationEvent;

        bool HasSubscribedEvent<TEvent>();

        IEnumerable<EventHandlerDescriber> GetEventHandlers<TEvent>()
            where TEvent : IIntegrationEvent;

        string GetEventKey<TEvent>() where TEvent : IIntegrationEvent;

        event EventHandler<EventBusArgs> OnEventRemoved;
    }

    internal class DefaultEventBusSubscriptionManager : DisposableObj, IEventBusSubscriptionManager
    {
        readonly ConcurrentDictionary<string, List<EventHandlerDescriber>> _dicHandlers;
        readonly SemaphoreSlim _lock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public event EventHandler<EventBusArgs> OnEventRemoved;

        public DefaultEventBusSubscriptionManager()
        {
            _dicHandlers = new ConcurrentDictionary<string, List<EventHandlerDescriber>>();
        }
        public async Task AddSubscription<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var key = GetEventName<TEvent>();

            await _lock.WaitAsync();
            try
            {
                var handlers = _dicHandlers.GetOrAdd(key, new List<EventHandlerDescriber> { });
                var handlerDesc = EventHandlerDescriber.Typed(typeof(TEventHandler));
                handlers.Add(handlerDesc);
            }
            finally
            {
                _lock.Release();
            }
        }

        public IEnumerable<EventHandlerDescriber> GetEventHandlers<TEvent>() 
            where TEvent : IIntegrationEvent
        {
            var key = GetEventName<TEvent>();
            _dicHandlers.TryGetValue(key, out var handlers);
            return handlers ?? Enumerable.Empty<EventHandlerDescriber>();
        }

        public async Task RemoveSubscription<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var key = GetEventName<TEvent>();
            await _lock.WaitAsync();
            try
            {
                if (_dicHandlers.TryGetValue(key, out var handlers))
                {
                    var desc = handlers.SingleOrDefault(p => p.HandlerType == typeof(TEventHandler));
                    handlers.Remove(desc);
                }
                if (!handlers.Any())
                    _dicHandlers.TryRemove(key, out var removedHandlers);
            }
            finally
            {
                _lock.Release();
            }
        }

        public bool HasSubscribedEvent<TEvent>()
        {
            return _dicHandlers.ContainsKey(GetEventName<TEvent>());
        }

        public string GetEventKey<TEvent>() where TEvent : IIntegrationEvent
        {
            return typeof(TEvent).Name;
        }

        protected override void Disposing()
        {
            ;
        }

        private string GetEventName<TEvent>() => typeof(TEvent).FullName;
    }
}