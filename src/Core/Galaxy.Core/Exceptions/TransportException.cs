using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core.Exceptions
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
