using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Galaxy.Core.Messaging
{
    public interface IMQConsumerClient : IDisposable
    {
        void Subscribe(IEnumerable<string> topics);
        void Listening(TimeSpan timeout, CancellationToken cancellationToken);
        void Commit();
        void Reject();
        event EventHandler<MQEventArgs> OnMessageReceived;
    }
}
