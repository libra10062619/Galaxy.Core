using System;

namespace Galaxy.Core.EventBus
{
    public class TransportException : Exception
    {
        public bool IsTimedOut { get; }

        public TransportException(string message = null, Exception innerException = null) : base(message, innerException)
        {
            IsTimedOut = innerException != null && innerException is TimeoutException;
        }
    }
}
