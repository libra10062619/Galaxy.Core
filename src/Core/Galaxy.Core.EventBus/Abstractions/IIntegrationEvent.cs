using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core.EventBus.Abstractions
{
    public interface IIntegrationEvent
    {
        string EventId { get; }
        DateTimeOffset Timestamp { get; }
        IDictionary<string, object> Metadata { get; set; }
    }
}
