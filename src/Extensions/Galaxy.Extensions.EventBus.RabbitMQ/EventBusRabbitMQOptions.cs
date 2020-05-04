using Galaxy.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Galaxy.Extensions.EventBus.RabbitMQ
{
    public sealed class EventBusRabbitMQOptions : ExtensionOptions
    {
        public string ClientProviderName { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name.ToLower();

        public string Host { get; set; } = "localhost";

        public string VirtualHost { get; set; } = "/";

        public int Port { get; set; } = -1;

        public string Username { get; set; } = "guest";

        public string Password { get; set; } = "guest";

        public string ExchangeType { get; set; } = "direct";

        public string ExchangeName { get; set; } = "galaxy.default.router";

        public int ChannelPoolSize { get; set; } = 50;

        public int RequestedConnectionTimeout { get; set; } = 60 * 1000;

        public int SocketReadTimeout { get; set; } = 60 * 1000;

        public int SocketWriteTimeout { get; set; } = 60 * 1000;

        public int MessageExpriesInMill { get; set; } = 15 * 24 * 3600 * 1000;

        public int RetryCount { get; set; } = 3;

        public bool AllowAsyncConsumer { get; set; } = true;

        public string QueueName { get; set; }

        public IDictionary<string, string> KeyValuePairs { get; set; }
    }
}
