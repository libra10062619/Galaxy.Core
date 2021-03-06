﻿using Galaxy.Core;
using Galaxy.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Galaxy.Core.Caching.Redis
{
    public class RedisCacheOptions
    {
        public string Configuration { get; set; }

        public string InstanceName { get; set; }
    }
}