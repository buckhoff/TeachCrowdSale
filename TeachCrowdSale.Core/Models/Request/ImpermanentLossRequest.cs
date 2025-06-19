using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Request
{
    public class ImpermanentLossRequest
    {
        public int PoolId { get; set; }
        public decimal Token0Amount { get; set; }
        public decimal Token1Amount { get; set; }
        public decimal PriceChangePercentage { get; set; }
        public decimal CurrentToken0Price { get; set; }
        public decimal CurrentToken1Price { get; set; }
    }
}
