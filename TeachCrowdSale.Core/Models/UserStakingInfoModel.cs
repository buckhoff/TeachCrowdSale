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
    /// User staking information
    /// </summary>
    public class UserStakingInfoModel
    {
        [EthereumAddress]
        public string WalletAddress { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal TotalStaked { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalRewards { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ClaimableRewards { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ProjectedMonthlyRewards { get; set; }

        public List<UserStakePositionModel> StakePositions { get; set; } = new();
        public SchoolBeneficiaryModel? SelectedSchool { get; set; }
        public List<RewardClaimHistoryModel> RewardHistory { get; set; } = new();
    }
}
