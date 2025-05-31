using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Burning
{
    public class BurnEventModel
    {
        public DateTime Date { get; set; }
        public long Amount { get; set; }
        public string Mechanism { get; set; } = string.Empty;
        public string TransactionHash { get; set; } = string.Empty;
        public decimal UsdValue { get; set; }
    }
}
