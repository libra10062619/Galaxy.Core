using Galaxy.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core.Messaging
{
    public class TransportResult
    {
        public bool Success { get; protected set; }

        public string Message { get; }

        public TransportException TransportException { get; protected set; }

        public static TransportResult Successful() => new TransportResult { Success = true };
        public static TransportResult Failed(TransportException exception)
        {
            return new TransportResult { Success = false, TransportException = exception };
        }
    }
}
