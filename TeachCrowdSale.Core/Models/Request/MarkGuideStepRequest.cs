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
    /// Request model for marking guide step as completed
    /// </summary>
    public class MarkGuideStepRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [Range(1, 10)]
        public int StepNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
