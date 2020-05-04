using Galaxy.Core.EventBus.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQEventBusSample.Events;

namespace RabbitMQEventBusSample.Handlers
{
    public class EventBusConfiguration
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IEventHandler<MemberCreatedEvent>, MemberCreatedEventHandler>();
            services.AddScoped<IEventHandler<MemberUpdatedEvent>, MemberUpdatedEventHandler>();
        }
        public static void Configure(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
            eventBus.Subscribe<MemberCreatedEvent, MemberCreatedEventHandler>();
            eventBus.Subscribe<MemberUpdatedEvent, MemberUpdatedEventHandler>();
        }
    }
}
