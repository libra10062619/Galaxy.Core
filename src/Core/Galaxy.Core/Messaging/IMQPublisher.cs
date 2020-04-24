using Galaxy.Core.MessagQueue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxy.Core.Messaging
{
    public interface IMQPublisher : IDisposable
    {
        Task<TransportResult> PublisAsync<TMessage>(TMessage message, CancellationToken token) where TMessage : IMQMessage;
    }
}
