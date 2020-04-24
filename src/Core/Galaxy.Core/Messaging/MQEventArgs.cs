using System;

namespace Galaxy.Core.Messaging
{
    public class MQEventArgs : EventArgs
    {
        public string EventName { get; set; }

        public byte[] Message { get; set; }

        public MQEventArgs(string eventName, byte[] message)
        {
            this.EventName = eventName;
            this.Message = message;
        }
    }
}