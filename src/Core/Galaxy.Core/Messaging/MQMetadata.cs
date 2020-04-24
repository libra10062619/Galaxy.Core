using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Galaxy.Core.Messaging
{
    public class MQMetadata
    {
        public string Id { get; }

        public string Routing { get; }

        public int Sequence { get; } = 1;

        public long Timestamp { get; } = DateTimeOffset.UtcNow.Ticks;

        public IDictionary<string, object> AdditionalData { get; }

        public MQMetadata(string id, int sequence, long Timestamp, IDictionary<string, object> additional)
        {
            Id = id?? throw new ArgumentNullException("");
        }
    }
}
