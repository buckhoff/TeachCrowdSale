using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class TreasuryTransaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty; // INFLOW, OUTFLOW
        public string? TransactionHash { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsVerified { get; set; }
    }
}
