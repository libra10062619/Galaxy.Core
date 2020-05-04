using Galaxy.Core.Abstractions;
using Galaxy.Core.EventBus;
using Galaxy.Core.EventBus.Abstractions;
using Galaxy.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxy.Extensions.EventBus.RabbitMQ
{
    internal class EventBusRabbitMQ : DisposableObj, IEventBus
    {
        readonly IConnectionManager _connectionManager; 
        readonly ILogger _logger;
        readonly IEventBusSubscriptionManager _subscriptionManager;
        readonly IServiceScopeFactory _serviceScope;
        readonly EventBusRabbitMQOptions _options;
        readonly IModel _consumerChannel;
        readonly JsonSerializerOptions _dynamicSerializerOptions = new JsonSerializerOptions { Converters = { new Core.Serialization.DynamicJsonConverter() } };

        public EventBusRabbitMQ(IConnectionManager connectionManager,
            IEventBusSubscriptionManager subscriptionManager,
            IServiceScopeFactory serviceScope,
            EventBusRabbitMQOptions options,
            ILogger<EventBusRabbitMQ> logger)
        {
            _connectionManager = connectionManager;
            _serviceScope = serviceScope;
            _options = options;
            _logger = logger;

            _subscriptionManager = subscriptionManager;
            InitSubscriptionManager(_subscriptionManager);
            InitConsumerChannel(_consumerChannel);            
        }

        public async Task<TransportResult> PublishAsync<TEvent>(TEvent @event, CancellationToken token = default) 
            where TEvent : IIntegrationEvent
        {
            var tcs = new TaskCompletionSource<TransportResult>(TaskCreationOptions.RunContinuationsAsynchronously);
            var eventName = @event.GetType().Name;
            var channel = _connectionManager.Rent();
            await Task.Yield();
            try
            {                
                var body = JsonSerializer.SerializeToUtf8Bytes(@event);
                channel.ConfirmSelect();    //打开客户端确认模式
                channel.ExchangeDeclare(_options.ExchangeName, _options.ExchangeType, durable: true);             
                var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_options.RetryCount, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)), (ex, time) =>
                    {
                        _logger.LogWarning(ex, "RabbitMQ Client could not connect after {timeout}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                    }
                );
                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;
                    properties.Headers = @event.Metadata;
                    channel.BasicPublish(_options.ExchangeName, routingKey : eventName, basicProperties : properties, body : body);
                });

                // 等待消息送达确认，若未送达，返回异常信息
                if (!channel.WaitForConfirms(timeout: TimeSpan.FromSeconds(2), out bool timedOut))
                {
                    TransportException exception = timedOut
                        ? new TransportException("Message not arrived cause of timeout", innerException: new TimeoutException())
                        : new TransportException("Message not arrived");
                    _logger.LogDebug($"{exception.Message}");

                    tcs.SetResult(TransportResult.Failed(exception));
                }
                tcs.SetResult(TransportResult.Successful());
            }
            catch (Exception ex)
            {
                tcs.SetResult(TransportResult.Failed(new TransportException(innerException: ex)));
            }
            finally
            {
                _connectionManager.Return(channel);
            }
            return await tcs.Task;
        }

        public async Task Subscribe<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {       
            if (!_subscriptionManager.HasSubscribedEvent<TEvent>())
            {
                var eventName = _subscriptionManager.GetEventKey<TEvent>();
                await Task.Yield();

                _connectionManager.TryConnect();
                var channel = _connectionManager.Rent();
                try
                {
                    channel.QueueBind(_options.QueueName, _options.ExchangeName, routingKey: eventName);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Can not bind queue [{_options.QueueName}] with routing key [{eventName}]. Error:{ex.StackTrace}");
                }
                finally
                {
                    _connectionManager.Return(channel);
                }

                _logger.LogInformation("Subscribing to event {0} with {1}", eventName, typeof(TEventHandler).GetGenericTypeName());

                await _subscriptionManager.AddSubscription<TEvent, TEventHandler>();
            }

            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (_consumerChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += async (sender, args) =>
                {
                    var eventName = args.RoutingKey;
                    var body = Encoding.UTF8.GetString(args.Body.Span);
                    try
                    {
                        if (body.ToLowerInvariant().Contains("throw-fake-exception"))
                        {
                            throw new InvalidOperationException($"Fake exception requested: \"{body}\"");
                        }

                        await ProcessEvent<TEvent>(eventName, body);
                        // 发送ACK，确认消息已消费，消息将从队列移除
                        _consumerChannel.BasicAck(args.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex)
                    {
                        // 若发生异常，消息将自动移动至死信队列。可参考https://www.rabbitmq.com/dlx.html
                        _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", body);
                    }                    
                };

                _consumerChannel.BasicConsume(
                    queue: _options.QueueName,
                    autoAck: false,
                    consumer: consumer);
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }

        public async Task Unsubscribe<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            if (_subscriptionManager.HasSubscribedEvent<TEvent>())
            {
                var eventName = _subscriptionManager.GetEventKey<TEvent>();

                await _subscriptionManager.RemoveSubscription<TEvent, TEventHandler>();
            }
            //await Task.CompletedTask;
        }

        protected override void Disposing()
        {
            _connectionManager.Dispose();
            _subscriptionManager.Dispose();
        }

        private async Task ProcessEvent<TEvent>(string eventName, string message)
            where TEvent : IIntegrationEvent
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            if (_subscriptionManager.HasSubscribedEvent<TEvent>())
            {
                using (var scope = _serviceScope.CreateScope())
                {
                    // 可使用dynamic类型调用handler
                    //dynamic eventData = JsonSerializer.Deserialize<dynamic>(message, _dynamicJson);

                    var serviceProvider = scope.ServiceProvider;
                    var subDescribers = _subscriptionManager.GetEventHandlers<TEvent>();

                    var eventData = JsonSerializer.Deserialize<TEvent>(message);
                    var handlers = serviceProvider.GetServices<IEventHandler<TEvent>>();
                    foreach (var handler in handlers)
                    {
                        await Task.Yield();
                        await handler.HandleAsync(eventData);
                    }
                }
            }
            else
            {
                _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
            }
        }

        /// <summary>
        /// 为Consumer创建独立的channel
        /// </summary>
        /// <returns></returns>
        private void InitConsumerChannel(IModel channel)
        {
            channel = _connectionManager.CreateModel();
            channel.ExchangeDeclare(_options.ExchangeName, _options.ExchangeType, durable: true);
            channel.QueueDeclare(queue: _options.QueueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        private void InitSubscriptionManager(IEventBusSubscriptionManager subscriptionManager)
        {
            subscriptionManager.OnEventRemoved += async (sender, args) =>
            {
                await Task.Yield();

                _logger.LogTrace($"Removed Subscription");
                _connectionManager.TryConnect();

                var channel = _connectionManager.Rent();
                channel.QueueUnbind(args.QueueName, exchange: _options.ExchangeName, routingKey: args.EventName);
            };
        }
    }
}
