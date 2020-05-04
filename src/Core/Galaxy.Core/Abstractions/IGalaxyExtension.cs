using Microsoft.Extensions.DependencyInjection;
using System;

namespace Galaxy.Core.Abstractions
{
    public interface IGalaxyExtension
    {
        void Register(IServiceCollection services);

        void Build(IServiceProvider serviceProvider);
    }

    public abstract class GalaxyExtension : IGalaxyExtension
    {
        public virtual void Build(IServiceProvider serviceProvider)
        {
        }

        public virtual void Register(IServiceCollection services)
        {
            RegisterServices(services);
        }

        protected abstract void RegisterServices(IServiceCollection services);
    }

    public abstract class GalaxyExtension<TOptions> : GalaxyExtension
        where TOptions : ExtensionOptions
    {
        public TOptions ExtOptions { get; }

        public GalaxyExtension(Action<TOptions> setupAction)
        {
            if (null == setupAction) throw new ArgumentNullException(nameof(setupAction));

            ExtOptions = Activator.CreateInstance<TOptions>();
            setupAction(ExtOptions);
        }

        public override void Register(IServiceCollection services)
        {
            services.AddOptions<TOptions>();
            services.AddSingleton(ExtOptions);
            RegisterServices(services);
        }
    }
}
