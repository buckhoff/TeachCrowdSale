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
    /// Request model for getting pool reserves
    /// </summary>
    public class PoolReservesRequest
    {
        [Required]
        [EthereumAddress]
        public string PoolAddress { get; set; } = string.Empty;

        [StringLength(50)]
        public string DexName { get; set; } = "quickswap";

        public bool IncludeTokenPrices { get; set; } = true;

        public bool IncludeTotalValue { get; set; } = true;

        public bool UseCache { get; set; } = true;
    }
}
