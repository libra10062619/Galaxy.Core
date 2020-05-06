using Galaxy.Core.Abstractions;
using Galaxy.Core.EventBus;
using Galaxy.Core.EventBus.Abstractions;
using Galaxy.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Galaxy.Extensions.EventBus.Kafka
{
    internal class EventBusKafka : DisposableObj, IEventBus
    {
        readonly ILogger _logger;
        readonly IServiceScopeFactory _serviceScope;
        readonly IEventBusSubscriptionManager _subscriptionManager;
        readonly IConnectionManager _connectionManager;
        readonly EventBusKafkaOptions _options;

        public EventBusKafka()
        {

        }

        public async Task<TransportResult> PublishAsync<TEvent>(TEvent @event, CancellationToken token = default) where TEvent : IIntegrationEvent
        {
            var eventName = @event.GetType().Name;
            await Task.Yield();
            try
            {
                var body = JsonSerializer.Serialize(@event);
                var message = new Confluent.Kafka.Message<Confluent.Kafka.Null, string> { Value = body };
                var producer = _connectionManager.Connection;
                var policy = Policy.Handle<SocketException>()
                    .Or<Confluent.Kafka.KafkaException>()
                    .WaitAndRetry(_options.RetryCount, retry => TimeSpan.FromSeconds(Math.Pow(2, retry)), (ex, time) =>
                    {
                        _logger.LogWarning(ex, "Kafka Client could not connect after {timeout}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                    }
                );
                policy.Execute(() =>
                {
                    producer.ProduceAsync(topic: eventName, message: message, token);
                });

                return TransportResult.Successful();
            }
            catch (Exception ex)
            {
                return TransportResult.Failed(new TransportException(innerException: ex));
            }
        }

        public async Task Subscribe<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            if (!_subscriptionManager.HasSubscribedEvent<TEvent>())
            {
                var eventName = _subscriptionManager.GetEventKey<TEvent>();
                await Task.Yield();

                var consumerConfig = new Confluent.Kafka.ConsumerConfig
                {
                    BootstrapServers = _options.Hosts,
                    GroupId = _options.GroupId,
                    EnableAutoCommit = false,
                    StatisticsIntervalMs = 5000,
                    SessionTimeoutMs = 6000,
                    AutoOffsetReset = Confluent.Kafka.AutoOffsetReset.Earliest,
                    EnablePartitionEof = true
                };

                var consumer = new Confluent.Kafka.ConsumerBuilder<Confluent.Kafka.Ignore, string>(consumerConfig)
                    .SetErrorHandler((_, h) =>
                    {
                    })
                    .Build();
                
                try
                {
                    consumer.Subscribe(new[] { eventName });
                    while (true)
                    {
                        try
                        {
                            var consumeResult = consumer.Consume();

                            if (consumeResult.IsPartitionEOF)
                            {
                                _logger.LogInformation($"Reached end of topic {consumeResult.Topic}, partition {consumeResult.Partition}, offset {consumeResult.Offset}.");
                                continue;
                            }
                            // 此处已经获得要消费的message
                            // TODO 消费message

                            if (consumeResult.Offset % _options.CommitPeriod == 0)
                            {
                                // The Commit method sends a "commit offsets" request to the Kafka
                                // cluster and synchronously waits for the response. This is very
                                // slow compared to the rate at which the consumer is capable of
                                // consuming messages. A high performance application will typically
                                // commit offsets relatively infrequently and be designed handle
                                // duplicate messages in the event of failure.
                                try
                                {
                                    consumer.Commit(consumeResult);
                                }
                                catch (Confluent.Kafka.KafkaException ex)
                                {
                                    _logger.LogError($"Commit error: {ex.Error.Reason}");
                                }
                            }
                        }
                        catch (Confluent.Kafka.ConsumeException ex)
                        {
                            _logger.LogError($"Commit error: {ex.Error.Reason}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Can not bind group [{_options.GroupId}] with topic key [{eventName}]. Error:{ex.StackTrace}");
                }
                finally
                {
                    ;// _connectionManager.Return(channel);
                }

                _logger.LogInformation("Subscribing to event {0} with {1}", eventName, typeof(TEventHandler).GetGenericTypeName());

                await _subscriptionManager.AddSubscription<TEvent, TEventHandler>();
            }
            
        }

        public async Task Unsubscribe<TEvent, TEventHandler>()
            where TEvent : IIntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            
        }

        protected override void Disposing()
        {
            ;
        }
    }
}
