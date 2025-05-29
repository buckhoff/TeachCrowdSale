using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Trading pair model
    /// </summary>
    public class TradingPairModel
    {
        public string PairName { get; set; } = string.Empty;
        public string DexName { get; set; } = string.Empty;
        public string DexLogo { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Liquidity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Volume24h { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string TradingUrl { get; set; } = string.Empty;
    }
}
