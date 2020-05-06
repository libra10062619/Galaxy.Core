using Galaxy.Core;
using Galaxy.Core.EventBus.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Extensions.EventBus.Kafka
{
    public static class EventBusKafkaExtensions
    {
        public static GalaxyBuilder WithKafkaEventBus(this GalaxyBuilder builder, Action<EventBusKafkaOptions> setup)
        {
            builder.SetupExtension(new EventBusKafkaExtension(setup));
            return builder;
        }
    }

    public sealed class EventBusKafkaExtension : EventBusExtension<EventBusKafkaOptions>
    {
        public EventBusKafkaExtension(Action<EventBusKafkaOptions> setup) : base(setup)
        { 
        }

        protected override void RegisterEventBus(IServiceCollection services)
        {
            throw new NotImplementedException();
        }
    }
}
