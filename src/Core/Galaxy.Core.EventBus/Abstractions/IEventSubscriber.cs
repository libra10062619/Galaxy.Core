using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Galaxy.Core.EventBus.Abstractions
{
    public interface IEventSubscriber : IDisposable
    {
        Task Subscribe<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>;

        /*Task Subscribe<TEvent>(IEnumerable<IEventHandler<TEvent>> eventHandlers)
            where TEvent : IIntegrationEvent;*/

        Task Unsubscribe<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>;
    }
}