using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    public class LiquidityTrendDataResponse
    {
        public DateTime Date { get; set; }
        public decimal TotalValueLocked { get; set; }
        public decimal ChangePercentage { get; set; }
    }
}
