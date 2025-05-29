using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Purchase option model
    /// </summary>
    public class PurchaseOptionModel
    {
        public string Method { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MinAmount { get; set; }

        [Range(0, double.MaxValue)]
        public decimal MaxAmount { get; set; }

        public string ProcessingTime { get; set; } = string.Empty;
        public string Fees { get; set; } = string.Empty;
    }
}
