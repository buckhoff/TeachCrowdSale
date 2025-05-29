using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Vesting information model
    /// </summary>
    public class VestingInfoModel
    {
        [PercentageRange]
        public int TgePercentage { get; set; }

        [Range(0, 60)]
        public int VestingMonths { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TgeTokens { get; set; }

        [Range(0, double.MaxValue)]
        public decimal VestedTokens { get; set; }
    }

}
