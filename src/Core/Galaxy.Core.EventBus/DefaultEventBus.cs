namespace Galaxy.Core.EventBus
{
    /*internal class DefaultEventBus : DisposableObj, IEventBus
    {
        readonly IEventPublisher _eventPublisher;
        readonly IEventBusSubscriptionManager _eventBusSubscriptionManager;
        readonly ILogger _logger;

        public DefaultEventBus(IEventPublisher eventPublisher,
            IEventBusSubscriptionManager eventBusSubscriptionManager,
            ILogger<DefaultEventBus> logger)
        {
            _eventPublisher = eventPublisher;
            _eventBusSubscriptionManager = eventBusSubscriptionManager;
            _logger = logger;
        }

        public Task<TransportResult> PublishAsync<TEvent>(TEvent @event, CancellationToken token = default)
            where TEvent : IIntegrationEvent
        {
            return _eventPublisher.PublishAsync(@event, token);
        }

        public async Task Subscribe<TEvent>(IEventHandler<TEvent> eventHandler)
            where TEvent : IIntegrationEvent
        {
            await _eventBusSubscriptionManager.AddSubscription(eventHandler);
        }

        public async Task Subscribe<TEvent>(IEnumerable<IEventHandler<TEvent>> eventHandlers)
            where TEvent : IIntegrationEvent
        {
            foreach (var item in eventHandlers)
                await _eventBusSubscriptionManager.AddSubscription(item);
        }

        public async Task Unsubscribe<TEvent>(IEventHandler<TEvent> eventHandler) where TEvent : IIntegrationEvent
        {
            await _eventBusSubscriptionManager.RemoveSubscription(eventHandler);
        }

        protected override void Disposing()
        {
            _eventPublisher.Dispose();
            _eventBusSubscriptionManager.Dispose();
        }
    }*/
}
