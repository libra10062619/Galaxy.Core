using Galaxy.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Galaxy.Core
{
    public sealed class GalaxyBuilder
    {
        readonly IServiceCollection _services;
        readonly List<IGalaxyExtension> _extenstions;

        public GalaxyBuilder(IServiceCollection services, Action<GalaxyOptions> setupAction)
        {
            if (null == setupAction) throw new ArgumentNullException(nameof(setupAction));
            var options = new GalaxyOptions();
            setupAction(options);

            this._services = services;
            this._extenstions = new List<IGalaxyExtension>();
        }

        public void RegisterServices()
        {
            _services.AddSingleton(this);
            _services.AddHttpClient();

            // TODO 注册所有的服务
        }

        public void SetupExtension(IGalaxyExtension extension)
        {
            _extenstions.Add(extension);
            extension.Register(_services);
        }

        public void Build(IServiceProvider serviceProvider)
        {
            _extenstions.ForEach(p => p.Build(serviceProvider));
        }
    }
}
