using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// Staking reward distribution to schools
    /// </summary>
    public class SchoolRewardDistribution
    {
        public int Id { get; set; }
        public int SchoolBeneficiaryId { get; set; }

        [EthereumAddress]
        public string StakerAddress { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        public DateTime DistributionDate { get; set; }

        [TransactionHash]
        public string TransactionHash { get; set; } = string.Empty;

        public Enum.TransactionStatus Status { get; set; }

        // Navigation properties
        public SchoolBeneficiary? SchoolBeneficiary { get; set; }
    }
}
