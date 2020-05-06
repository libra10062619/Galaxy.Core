using Galaxy.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core.EventBus.Abstractions
{
    public abstract class EventBusExtension<TOptions> : GalaxyExtension<TOptions> where TOptions : ExtensionOptions
    {
        public EventBusExtension(Action<TOptions> setup) : base(setup)
        {
        }
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IEventBusSubscriptionManager, DefaultEventBusSubscriptionManager>();
            RegisterEventBus(services);
        }

        protected abstract void RegisterEventBus(IServiceCollection services);
    }
}
