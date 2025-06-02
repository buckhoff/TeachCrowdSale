using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Burning
{
    /// <summary>
    /// Burn event model for web consumption
    /// Maps from BurnEvent entity
    /// </summary>
    public class BurnEventModel
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [Range(0, double.MaxValue)]
        public long Amount { get; set; }

        public string Mechanism { get; set; } = string.Empty;

        [TransactionHash]
        public string TransactionHash { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal UsdValue { get; set; }

        public string? Description { get; set; }

        public bool IsVerified { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
