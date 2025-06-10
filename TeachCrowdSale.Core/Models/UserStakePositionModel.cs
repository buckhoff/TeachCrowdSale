using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// User stake position model
    /// </summary>
    public class UserStakePositionModel
    {
        public int StakeId { get; set; }
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal StakedAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal AccruedRewards { get; set; }

        [Range(0, 200)]
        public decimal CurrentAPY { get; set; }

        public DateTime StakeDate { get; set; }
        public DateTime UnlockDate { get; set; }
        public bool CanUnstake { get; set; }
        public bool CanClaim { get; set; }
        public bool CanCompound { get; set; }

        [Range(0, 365)]
        public int DaysRemaining { get; set; }

        public string Status { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;

        public SchoolBeneficiaryModel SchoolBeneficiary { get; set; } = new SchoolBeneficiaryModel();
    }
}
