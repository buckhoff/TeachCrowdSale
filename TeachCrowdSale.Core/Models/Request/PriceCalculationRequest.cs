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
    /// Price calculation request model
    /// </summary>
    public class PriceCalculationRequest
    {
        [Required(ErrorMessage = "Wallet address is required")]
        [EthereumAddress]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tier ID is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Tier ID must be a valid number")]
        public int TierId { get; set; }

        [Required(ErrorMessage = "USD amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.01")]
        public decimal UsdAmount { get; set; }
    }
}
