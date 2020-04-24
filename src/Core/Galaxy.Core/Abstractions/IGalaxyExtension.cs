using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

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
}
