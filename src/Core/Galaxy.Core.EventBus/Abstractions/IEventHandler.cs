using Galaxy.Core.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxy.Core.EventBus.Abstractions
{
    public interface IEventHandler : IDisposable
    {
        //Task<bool> HandleAsync(IEvent @event, CancellationToken cancellationToken = default);
    }

    public interface IDynamicEventHandler : IEventHandler
    {
        Task HandleAsync(dynamic @event, CancellationToken cancellationToken = default);
    }

    public interface IEventHandler<in TEvent> : IEventHandler where TEvent : IIntegrationEvent
    {
        Task<bool> HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }

    public abstract class BaseEventHandler<TEvent> : DisposableObj, IEventHandler<TEvent> where TEvent : IIntegrationEvent
    {
        public abstract Task<bool> HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}

