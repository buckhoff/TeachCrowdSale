using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Tier purchase information
    /// </summary>
    public class TierPurchaseInfo
    {
        public int TierId { get; set; }
        public string TierName { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal UsdAmount { get; set; }
    }
}
