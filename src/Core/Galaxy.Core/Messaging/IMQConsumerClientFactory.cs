using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core.Messaging
{
    public interface IMQConsumerClientFactory
    {
        IMQConsumerClient Create(string queueName);
    }
}
