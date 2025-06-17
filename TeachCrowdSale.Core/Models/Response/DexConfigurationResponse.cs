using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for DEX configuration data
    /// Maps from DexConfiguration entity for API consumption
    /// </summary>
    public class DexConfigurationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public bool IsActive { get; set; }
        public decimal DefaultFeePercentage { get; set; }
        public string Network { get; set; } = string.Empty;
        public string RouterAddress { get; set; } = string.Empty;
        public string FactoryAddress { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
