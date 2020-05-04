using Galaxy.Core.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQEventBusSample.Events
{
    public class MemberCreatedEvent : IIntegrationEvent
    {
        public string EventId => Guid.NewGuid().ToString();

        public DateTimeOffset Timestamp { get; set; }

        public IDictionary<string, object> Metadata { get; set; }

        public string MemberId { get; set; }

        public string MemberName { get; set; }
    }

    public class MemberUpdatedEvent : IIntegrationEvent
    {
        public string EventId => Guid.NewGuid().ToString();

        public DateTimeOffset Timestamp { get; set; }

        public IDictionary<string, object> Metadata { get; set; }

        public string MemberId { get; set; }

        public string MemberName { get; set; }
    }
}
