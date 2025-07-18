﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Pool performance data for API response
    /// </summary>
    public class PoolPerformanceDataResponse
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string TokenPair { get; set; } = string.Empty;
        public string Token0Address { get; set; } = string.Empty; 
        public string Token1Address { get; set; } = string.Empty; 
        public string DexName { get; set; } = string.Empty;
        public decimal APY { get; set; }
        public decimal TotalValueLocked { get; set; }
        public decimal Volume24h { get; set; }
        public decimal FeesGenerated24h { get; set; }
        public decimal PriceChange24h { get; set; }
        public int ProvidersCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
