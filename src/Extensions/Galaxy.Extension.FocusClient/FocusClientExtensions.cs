using Galaxy.Core;
using Galaxy.Core.Abstractions;
using Galaxy.Extension.FocusClient;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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

    internal class FocusClientExtension : GalaxyExtension
    {
        readonly FocusClientOptions _focusClientOptions;
        public FocusClientExtension(Action<FocusClientOptions> setupAction)
        {
            if (null == setupAction) throw new ArgumentNullException(nameof(setupAction));

            var options = new FocusClientOptions();
            setupAction(options);
            this._focusClientOptions = options;
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddOptions<FocusClientOptions>();
            services.AddSingleton(_focusClientOptions);
            services.AddSingleton<FocusClientRegister>();
            // TODO 注册所有Rabbit需要的服务
        }

        public override void Build(IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<FocusClientRegister>().Register();
            //base.Build(serviceProvider);
        }
    }
}

