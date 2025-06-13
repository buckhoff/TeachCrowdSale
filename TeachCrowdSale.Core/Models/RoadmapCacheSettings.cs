using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Cache configuration settings for roadmap data
    /// </summary>
    public class RoadmapCacheSettings
    {
        public int ShortCacheDurationMinutes { get; set; } = 5;
        public int MediumCacheDurationMinutes { get; set; } = 15;
        public int LongCacheDurationMinutes { get; set; } = 60;
        public int MaxCacheSize { get; set; } = 1000;
        public bool EnableCaching { get; set; } = true;
        public bool EnableDistributedCache { get; set; } = false;
    }
}
