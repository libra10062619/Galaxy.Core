using Galaxy.Core.Caching.Redis;

namespace Galaxy.Core
{
    public sealed class GalaxyOptions
    {
        public RedisCacheOptions RedisCacheOptions { get; set; }
    }
}
