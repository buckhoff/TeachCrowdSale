using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Top staker model
    /// </summary>
    public class TopStakerModel
    {
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        public string DisplayAddress { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal TotalStaked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalRewards { get; set; }

        public int Rank { get; set; }
        public DateTime FirstStakeDate { get; set; }
    }
}
