using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core.EventBus
{
    public class EventBusArgs : EventArgs
    {
        public string EventName { get; set; }
        public string QueueName { get; set; } 
    }
}
