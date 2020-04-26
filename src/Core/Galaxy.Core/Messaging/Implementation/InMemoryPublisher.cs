using Galaxy.Core.Abstractions;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxy.Core.Messaging.Implementation
{
    internal class InMemoryPublisher : DisposableObj, IMQPublisher
    {
        readonly InMemoryMQMananger _memoryMQManager;
        readonly ILogger _logger;

        public InMemoryPublisher(InMemoryMQMananger memoryMQMananger,
            ILogger<InMemoryMQMananger> logger)
        {
            _memoryMQManager = memoryMQMananger;
            _logger = logger;
        }

        public async Task<TransportResult> PublisAsync<TMessage>(TMessage message, CancellationToken token = default) where TMessage : IMQMessage
        {
            _memoryMQManager.AddMessage(message);
            return await Task.FromResult(TransportResult.Successful());
        }

        protected override void Disposing()
        {
            _memoryMQManager.Dispose();
            _logger.LogDebug($"InMemoryPublisher is disposed.");
        }
    }
}
