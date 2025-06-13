using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Infrastructure.Configuration
{
    public class CacheConfiguration
    {
        public int ShortCacheDurationSeconds { get; set; } = 30;
        public int MediumCacheDurationSeconds { get; set; } = 120;
        public int LongCacheDurationSeconds { get; set; } = 600;
        public int MaxCacheSize { get; set; } = 1000;
        public bool EnableDistributedCache { get; set; } = false;
        public string? DistributedCacheConnectionString { get; set; }
    }
}
