using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Reward claim history model
    /// </summary>
    public class RewardClaimHistoryModel
    {
        public int ClaimId { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ClaimedAmount { get; set; }

        public DateTime ClaimDate { get; set; }

        [TransactionHash]
        public string TransactionHash { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
        public string PoolName { get; set; } = string.Empty;
        public string RewardType { get; set; } = string.Empty; // e.g. "Daily", "Compound"


    }
}
