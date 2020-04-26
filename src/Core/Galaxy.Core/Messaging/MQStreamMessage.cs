using System.Collections.Generic;

namespace Galaxy.Core.Messaging
{
    public abstract class MQStreamMessage : IMQMessage
    {
        public MQMetadata Metadata { get; protected set; }

        public byte[] Body { get; protected set; }

        public MQStreamMessage(IEnumerable<KeyValuePair<string, string>> metadata, byte[] body)
        {
        }

        public string GetMessageKey()
        {
            return Metadata.AdditionalData["Key"].ToString();
        }
    }
}
