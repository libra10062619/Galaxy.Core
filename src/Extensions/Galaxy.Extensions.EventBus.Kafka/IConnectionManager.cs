using Confluent.Kafka;
using Galaxy.Core.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Extensions.EventBus.Kafka
{
    public interface IConnectionManager : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        public IProducer<Null, string> Connection { get };

        /* IModel Rent();

        bool Return(IModel channel);

        IModel CreateModel();*/
    }

    internal class ConnectionManager : DisposableObj, IConnectionManager
    {
        readonly EventBusKafkaOptions _options;
        readonly ProducerConfig _producerConfig;
        IProducer<Null, string> _connection;
        public bool IsConnected => !_disposed && _connection != null;

        public IProducer<Null, string> Connection
        {
            get
            {
                TryConnect(); return _connection;
            }
        }

    public ConnectionManager(EventBusKafkaOptions options)
        {
            _options = options;

            _producerConfig = new ProducerConfig
            {
                BootstrapServers = _options.Hosts,
                ClientId = System.Net.Dns.GetHostName(),
            };
        }

        public bool TryConnect()
        {
            if (!IsConnected)
            {
                _connection = new ProducerBuilder<Null, string>(_producerConfig).Build();
            }
            return true;
        }

        protected override void Disposing()
        {
            _connection.Dispose();
        }
    }
}
