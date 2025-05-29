using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// User trade information model
    /// </summary>
    public class UserTradeInfoModel
    {
        [EthereumAddress]
        public string Address { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal TotalTokensPurchased { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalUsdSpent { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ClaimableTokens { get; set; }

        public DateTime? LastClaimTime { get; set; }
        public DateTime? NextVestingDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal NextVestingAmount { get; set; }

        public List<TierPurchaseInfo> TierPurchases { get; set; } = new();
    }
}
