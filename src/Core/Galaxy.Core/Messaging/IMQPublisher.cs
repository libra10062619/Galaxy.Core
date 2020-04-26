using System;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxy.Core.Messaging
{
    public interface IMQPublisher : IDisposable
    {
        Task<TransportResult> PublisAsync<TMessage>(TMessage message, CancellationToken token) where TMessage : IMQMessage;
    }
}
