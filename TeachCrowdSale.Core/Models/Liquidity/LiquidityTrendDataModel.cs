using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Liquidity
{
    public class LiquidityTrendDataModel
    {
        public DateTime Date { get; set; }
        public decimal TotalValueLocked { get; set; }
        public decimal Volume { get; set; }
        public decimal APY { get; set; }
    }
}
