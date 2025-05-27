using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class PurchaseTransaction
    {
        public int Id { get; set; }
        public string WalletAddress { get; set; } = string.Empty;
        public int TierId { get; set; }
        public decimal UsdAmount { get; set; }
        public decimal TokenAmount { get; set; }
        public decimal TokenPrice { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public TransactionStatus Status { get; set; }
    }
}
