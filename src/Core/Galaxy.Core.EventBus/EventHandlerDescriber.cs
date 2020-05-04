using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core.EventBus
{
    public class EventHandlerDescriber
    {
        public bool IsDynamic { get; }

        public string HandlerName { get; }

        public Type HandlerType { get; }

        private EventHandlerDescriber(Type handlerType, string handlerName = null, bool isDynamic = false)
        {
            HandlerType = handlerType;
            HandlerName = handlerName ?? handlerType.Name;
            IsDynamic = isDynamic;
        }

        public static EventHandlerDescriber Typed(Type type, string name = null) => new EventHandlerDescriber(type, name);

        public static EventHandlerDescriber Dynamic(Type type, string name = null) => new EventHandlerDescriber(type, name, true);
    }
}
