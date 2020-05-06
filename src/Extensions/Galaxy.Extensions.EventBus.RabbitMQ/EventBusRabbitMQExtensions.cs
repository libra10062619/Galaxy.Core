using Galaxy.Core.Abstractions;
using Galaxy.Core.EventBus;
using Galaxy.Extensions.EventBus.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Galaxy.Core.EventBus.Abstractions;
using Galaxy.Core;

namespace Galaxy.Extensions.DependencyInjection
{
    public static class EventBusRabbitMQExtensions
    {
        public static GalaxyBuilder WithRabbitEventBus(this GalaxyBuilder builder, Action<EventBusRabbitMQOptions> setup)
        {
            builder.SetupExtension(new EventBusRabbitMQExtension(setup));
            return builder;
        }
    }

    public sealed class EventBusRabbitMQExtension : EventBusExtension<EventBusRabbitMQOptions>
    {
        public EventBusRabbitMQExtension(Action<EventBusRabbitMQOptions> setup) : base(setup)
        { 
        }

        protected override void RegisterEventBus(IServiceCollection services)
        {
            services.AddSingleton<IConnectionManager, ConnectionManager>();
            services.AddSingleton<IEventBus, EventBusRabbitMQ>();            
        }
    }
}
