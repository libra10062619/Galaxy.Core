using Galaxy.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class GalaxyServiceCollectionExtensions
    {
        public static GalaxyBuilder AddGalaxy(this IServiceCollection services, Action<GalaxyOptions> setupAction)
        {
            var builder = new GalaxyBuilder(services, setupAction);

            builder.RegisterServices();

            return builder;
        }
    }
}
