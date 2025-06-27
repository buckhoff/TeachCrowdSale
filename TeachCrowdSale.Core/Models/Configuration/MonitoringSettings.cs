using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    public class MonitoringSettings
    {
        public int HealthCheckIntervalMinutes { get; set; } = 5;
        public AlertThresholds AlertThresholds { get; set; } = new();
    }

    public class AlertThresholds
    {
        public int HighTransactionVolumePerHour { get; set; } = 1000;
        public decimal LowLiquidityThresholdUSD { get; set; } = 50000;
        public decimal HighPriceDeviationPercentage { get; set; } = 10.0m;
        public decimal SystemErrorRatePercentage { get; set; } = 5.0m;
    }
}
