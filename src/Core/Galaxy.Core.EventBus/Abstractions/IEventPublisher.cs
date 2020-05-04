using System;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxy.Core.EventBus.Abstractions
{
    public interface IEventPublisher : IDisposable
    {
        Task<TransportResult> PublishAsync<TEvent>(TEvent @event, CancellationToken token = default) where TEvent : IIntegrationEvent;
    }
}