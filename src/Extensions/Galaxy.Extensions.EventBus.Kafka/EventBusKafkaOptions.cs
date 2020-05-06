using Galaxy.Core.Abstractions;
using System;
using System.Reflection;

namespace Galaxy.Extensions.EventBus.Kafka
{
    public class EventBusKafkaOptions : ExtensionOptions
    {
        public string ClientProviderName { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name.ToLower();

        public string Hosts { get; set; } = "localhost";

        public string GroupId { get; set; }

        public int RetryCount { get; set; } = 3;

        public int CommitPeriod { get; set; } = 5;
    }
}
