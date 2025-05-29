using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Price calculation response model
    /// </summary>
    public class PriceCalculationModel
    {
        public int TierId { get; set; }
        public string TierName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public decimal UsdAmount { get; set; }

        [Range(0.00000001, double.MaxValue)]
        public decimal TokenPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TokensToReceive { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PlatformFee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal NetworkFee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalFees { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalCost { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinPurchase { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxPurchase { get; set; }

        [Range(0, double.MaxValue)]
        public decimal UserExistingTokens { get; set; }

        public VestingInfoModel VestingInfo { get; set; } = new();
        public bool IsValid { get; set; }
    }
}
