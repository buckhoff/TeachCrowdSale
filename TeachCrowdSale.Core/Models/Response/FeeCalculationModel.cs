using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Fee calculation model
    /// </summary>
    public class FeeCalculationModel
    {
        [Range(0, double.MaxValue)]
        public decimal PlatformFee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal NetworkFee { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalFees { get; set; }
    }
}
