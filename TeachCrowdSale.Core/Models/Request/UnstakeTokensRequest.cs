﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Request
{
    /// <summary>
    /// Unstake tokens request model
    /// </summary>
    public class UnstakeTokensRequest
    {
        [Required(ErrorMessage = "Wallet address is required")]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Stake ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Stake ID must be a valid positive number")]
        public int StakeId { get; set; }

        public decimal? Amount { get; set; } // Null for full unstake
        public bool ForceEarlyWithdrawal { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
    }
}
