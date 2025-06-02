using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Vesting
{
    /// <summary>
    /// Vesting information model for web consumption
    /// Maps from VestingEvent entity
    /// </summary>
    public class VestingInfoModel
    {
        public int Id { get; set; }

        public DateTime UnlockDate { get; set; }

        [Range(0, double.MaxValue)]
        public long TokensUnlocked { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [TransactionHash]
        public string? TransactionHash { get; set; }

        public bool IsProcessed { get; set; }

        public DateTime CreatedAt { get; set; }

        [PercentageRange]
        public int TgePercentage { get; set; }

        [Range(0, 60)]
        public int VestingMonths { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TgeTokens { get; set; }

        [Range(0, double.MaxValue)]
        public decimal VestedTokens { get; set; }
    }
}
