using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class BurnEvent
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public long Amount { get; set; }
        public string Mechanism { get; set; } = string.Empty;
        public string TransactionHash { get; set; } = string.Empty;
        public decimal UsdValue { get; set; }
        public string? Description { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
