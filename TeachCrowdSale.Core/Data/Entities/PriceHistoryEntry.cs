using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class PriceHistoryEntry
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public decimal Volume { get; set; }
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty; // DEX, CEX, Oracle
        public string Pair { get; set; } = string.Empty; // TEACH/USDC, TEACH/ETH
    }
}
