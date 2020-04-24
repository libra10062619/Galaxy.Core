using Galaxy.Core.Caching.Redis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Galaxy.Core
{
    public sealed class GalaxyOptions
    {
        public RedisCacheOptions RedisCacheOptions { get; set; }
    }
}
