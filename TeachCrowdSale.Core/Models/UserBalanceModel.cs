using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models
{
    public class UserBalanceModel
    {
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal TotalPurchased { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalClaimed { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PendingTokens { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Tokens { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UsdAmount { get; set; }

        public List<decimal> TierAmounts { get; set; } = new();

        public DateTime? LastClaimTime { get; set; }

        public DateTime LastUpdated { get; set; }

        public List<PurchaseTransactionModel> Purchases { get; set; } = new();

        public List<ClaimTransactionModel> Claims { get; set; } = new();
    }
}
