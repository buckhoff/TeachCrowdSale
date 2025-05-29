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
    /// Tier selection request model
    /// </summary>
    public class TierSelectionRequest
    {
        [Required(ErrorMessage = "Tier ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Tier ID must be a positive number")]
        public int TierId { get; set; }

        [EthereumAddress]
        public string? UserAddress { get; set; }
    }
}
