using Galaxy.Core.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQEventBusSample.DTO;
using RabbitMQEventBusSample.Events;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQEventBusSample.Handlers
{
    public class MemberCreatedEventHandler : IEventHandler<MemberCreatedEvent>
    {
        readonly IRepository<MemberInfo, string> _repository;
        readonly ILogger _logger;

        public MemberCreatedEventHandler(IRepository<MemberInfo, string> repository,
            ILogger<MemberUpdatedEventHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public void Dispose()
        {
            ;
        }

        public async Task<bool> HandleAsync(MemberCreatedEvent @event, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            var id = @event.MemberId;
            var item = _repository.Get(id);
            item.Audit = $"MemberCreatedEvent事件[{JsonSerializer.Serialize(@event)}]";
            _repository.Update(item);
            return true;
        }
    }

    public class MemberUpdatedEventHandler : IEventHandler<MemberUpdatedEvent>
    {
        readonly IRepository<MemberInfo, string> _repository;
        readonly ILogger _logger;

        public MemberUpdatedEventHandler(IRepository<MemberInfo, string> repository,
            ILogger<MemberUpdatedEventHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public void Dispose()
        {
            ;
        }

        public async Task<bool> HandleAsync(MemberUpdatedEvent @event, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            var id = @event.MemberId;
            var item = _repository.Get(id);
            item.Audit = $"MemberUpdatedEvent事件[{JsonSerializer.Serialize(@event)}]";
            _repository.Update(item);
            return true;
        }
    }
}
