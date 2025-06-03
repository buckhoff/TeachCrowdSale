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
    /// User's selected school beneficiary for staking rewards
    /// </summary>
    public class UserStakingBeneficiary
    {
        public int Id { get; set; }

        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        public int SchoolBeneficiaryId { get; set; }
        public DateTime SelectedAt { get; set; }
        public bool IsActive { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalDonated { get; set; }

        // Navigation properties
        public SchoolBeneficiary SchoolBeneficiary { get; set; }
    }
}
