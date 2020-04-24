using Galaxy.Core.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core.MessagQueue
{
    public interface IMQMessage
    {
        MQMetadata Metadata { get; }

        byte[] Body { get; }

        string GetMessageKey();
    }
}
