using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    public class OracleSettings
    {
        public bool Enabled { get; set; } = true;
        public List<OracleProvider> Providers { get; set; } = new();
        public int UpdateIntervalMinutes { get; set; } = 5;
        public decimal PriceDeviationThresholdPercentage { get; set; } = 5.0m;
    }

    public class OracleProvider
    {
        public string Name { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int Priority { get; set; } = 1;
    }
}
