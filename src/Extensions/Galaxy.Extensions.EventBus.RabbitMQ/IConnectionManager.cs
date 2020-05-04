using Galaxy.Core.Abstractions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Galaxy.Extensions.EventBus.RabbitMQ
{
    public interface IConnectionManager : IDisposable
    {
        bool IsConnected { get; }

        IConnection TryConnect();

        IModel Rent();

        bool Return(IModel channel);

        IModel CreateModel();
    }

    internal sealed class ConnectionManager : DisposableObj, IConnectionManager
    {
        readonly EventBusRabbitMQOptions _options;
        readonly ConcurrentQueue<IModel> _channelPool = new ConcurrentQueue<IModel>();
        readonly ILogger _logger;
        readonly IConnectionFactory _connectionFactory;

        private IConnection _connection;
        private int _count;

        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

        public ConnectionManager(EventBusRabbitMQOptions options,
            ILogger<ConnectionManager> logger)
        {
            _logger = logger;
            _options = options;

            _connectionFactory = new ConnectionFactory()
            {
                UserName = options.Username,
                Port = options.Port,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(options.RequestedConnectionTimeout),
                SocketReadTimeout = TimeSpan.FromSeconds(options.SocketReadTimeout),
                SocketWriteTimeout = TimeSpan.FromSeconds(options.SocketWriteTimeout),
                DispatchConsumersAsync = options.AllowAsyncConsumer
            };
        }

        public IConnection TryConnect()
        {
            if (IsConnected) return _connection;

            _connection = _connectionFactory.CreateConnection(_options.Host.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), _options.ClientProviderName);
            _connection.ConnectionBlocked += (sender, args) =>
            {
                if (_disposed) return;
                _logger.LogWarning("");
                TryConnect();
            };
            _connection.ConnectionShutdown += (sender, args) =>
            {
                if (_disposed) return;
                _logger.LogWarning($"RabbitMQ client connection closed! --> {args.ReplyText}");
                TryConnect();
            };
            _connection.ConnectionShutdown += (sender, args) =>
            {
                if (_disposed) return;
                _logger.LogWarning("");
                TryConnect();
            };

            return _connection;
        }

        public IModel CreateModel()
        {
            TryConnect();
            return _connection.CreateModel();
        }

        public IModel Rent()
        {
            IModel channel;
            int tryTimes = 0;
            while (!_channelPool.TryDequeue(out channel))
            {
                if (tryTimes <= 100)
                    Thread.SpinWait(1);
                else
                    return _connection.CreateModel();

                tryTimes++;
            }

            Interlocked.Decrement(ref _count);
            Debug.Assert(_count >= 0);

            return channel;
        }

        public bool Return(IModel channel)
        {
            if (Interlocked.Increment(ref _count) <= _options.ChannelPoolSize)
            {
                _channelPool.Enqueue(channel);
                return true;
            }

            Interlocked.Decrement(ref _count);
            Debug.Assert(_channelPool.Count <= _options.ChannelPoolSize);

            channel.Close();
            channel.Dispose();

            return false;
        }

        protected override void Disposing()
        {
            while (_channelPool.TryDequeue(out IModel channel))
            {
                channel.Close();
                channel.Dispose();
            }
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
