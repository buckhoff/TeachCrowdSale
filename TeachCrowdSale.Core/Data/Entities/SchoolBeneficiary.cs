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
    /// School beneficiary configuration
    /// </summary>
    public class SchoolBeneficiary
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        public string ContactEmail { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int StudentCount { get; set; }

        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime VerifiedAt { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalReceived { get; set; }

        // Navigation properties
        public List<UserStakingBeneficiary> UserStakes { get; set; } = new();
    }
}
