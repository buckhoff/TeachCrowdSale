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
    /// Wallet connection request model
    /// </summary>
    public class WalletConnectionRequest
    {
        [Required(ErrorMessage = "Wallet address is required")]
        [EthereumAddress]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Signature is required")]
        public string Signature { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        public string Message { get; set; } = string.Empty;
    }
}
