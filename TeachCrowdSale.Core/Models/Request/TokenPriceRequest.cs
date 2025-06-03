using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// Request model for getting token price from DEX
    /// </summary>
    public class TokenPriceRequest
    {
        [Required]
        [EthereumAddress]
        public string TokenAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public string DexName { get; set; } = "quickswap";

        [StringLength(10)]
        public string QuoteCurrency { get; set; } = "USDC";

        public bool UseCache { get; set; } = true;

        [Range(1, 60)]
        public int CacheMinutes { get; set; } = 5;
    }
}
