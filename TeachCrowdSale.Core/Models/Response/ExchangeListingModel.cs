using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Exchange listing model
    /// </summary>
    public class ExchangeListingModel
    {
        public string ExchangeName { get; set; } = string.Empty;
        public string ExchangeLogo { get; set; } = string.Empty;
        public string ListingType { get; set; } = string.Empty;
        public DateTime EstimatedDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
