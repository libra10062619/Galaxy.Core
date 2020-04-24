using Galaxy.Core.Abstractions;
using Galaxy.Core.Caching;
using Galaxy.Core.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Galaxy.Core.Extensions
{
    public static class RedisCacheExtensions
    {
        public static GalaxyBuilder WithRedisCache(this GalaxyBuilder builder, Action<RedisCacheOptions> setupAction)
        {
            builder.SetupExtension(new RedisCacheExtension(setupAction));

            return builder;
        }
    }

    internal class RedisCacheExtension : GalaxyExtension
    {
        readonly RedisCacheOptions _redisOption;
        public RedisCacheExtension(Action<RedisCacheOptions> setupAction)
        {
            if (null == setupAction) throw new ArgumentNullException(nameof(setupAction));

            var options = new RedisCacheOptions();
            setupAction(options);
            _redisOption = options;
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton(_redisOption);
            services.AddSingleton<IRedisConnectionWrapper, RedisConnectionWrapper>();
            services.AddSingleton<ICacheManager, RedisCacheManager>();
            // TODO 注册所有Rabbit需要的服务
        }
    }
}
