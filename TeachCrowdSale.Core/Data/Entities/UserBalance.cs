using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class UserBalance
    {
        public int Id { get; set; }
        public string WalletAddress { get; set; } = string.Empty;
        public decimal TotalPurchased { get; set; }
        public decimal TotalClaimed { get; set; }
        public decimal PendingTokens { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<PurchaseTransaction> Purchases { get; set; } = new();
        public List<ClaimTransaction> Claims { get; set; } = new();
    }
}
