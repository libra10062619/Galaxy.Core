using Galaxy.Core;
using Galaxy.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class GalaxyAppBuilderExtensions
    {
        public static IApplicationBuilder UseGalaxy(this IApplicationBuilder app)
        {
            Ensure.NotNull(app);
            
            var galaxy = app.ApplicationServices.GetRequiredService<GalaxyBuilder>();

            galaxy.Build(app.ApplicationServices);
            
            return app;
        }
    }
}
