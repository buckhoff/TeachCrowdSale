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
    /// Request model for adding liquidity
    /// </summary>
    public class AddLiquidityRequest
    {
        [Required]
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int PoolId { get; set; }

        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Token0Amount { get; set; }

        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Token1Amount { get; set; }

        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Token0AmountMin { get; set; }

        [Required]
        [Range(0.000001, double.MaxValue)]
        public decimal Token1AmountMin { get; set; }

        [Range(0.1, 100)]
        public decimal SlippageTolerance { get; set; } = 0.5m;

        public bool ConfirmRisks { get; set; } = false;

        [Range(1, 60)]
        public int DeadlineMinutes { get; set; } = 20;
    }
}
