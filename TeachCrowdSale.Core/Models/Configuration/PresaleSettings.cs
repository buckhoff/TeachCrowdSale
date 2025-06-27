using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    public class PresaleSettings
    {
        public decimal SoftCapUSD { get; set; }
        public decimal HardCapUSD { get; set; }
        public decimal MinPurchaseUSD { get; set; }
        public decimal MaxPurchaseUSD { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool VestingEnabled { get; set; } = true;
        public int TGEPercentage { get; set; } = 20;
        public int VestingMonths { get; set; } = 6;
        public List<PresaleTier> Tiers { get; set; } = new();
    }

    public class PresaleTier
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PriceUSD { get; set; }
        public long Allocation { get; set; }
        public int TGEPercentage { get; set; } = 20;
        public int VestingMonths { get; set; } = 6;
    }
}
