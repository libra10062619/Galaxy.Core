using Galaxy.Core;
using Galaxy.Core.Abstractions;
using Galaxy.Extensions.FocusClient;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Galaxy.Extensions.DependencyInjection
{
    public static class FocusClientExtensions
    {
        public static GalaxyBuilder WithFocusGateway(this GalaxyBuilder builder, Action<FocusClientOptions> setupAction)
        {
            builder.SetupExtension(new FocusClientExtension(setupAction));

            return builder;
        }
    }

    internal class FocusClientExtension : GalaxyExtension<FocusClientOptions>
    {
        public FocusClientExtension(Action<FocusClientOptions> setupAction): base(setupAction)
        {
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<FocusClientRegister>();
            // TODO 注册所有Rabbit需要的服务
        }

        public override void Build(IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<FocusClientRegister>().Register();
        }
    }
}

