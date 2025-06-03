using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    /// <summary>
    /// DEX configuration and metadata
    /// </summary>
    public class DexConfiguration
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(200)]
        public string BaseUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string ApiUrl { get; set; } = string.Empty;

        [StringLength(200)]
        public string LogoUrl { get; set; } = string.Empty;

        [Range(0, 10)]
        public decimal DefaultFeePercentage { get; set; } = 0.3m;

        public bool IsActive { get; set; } = true;
        public bool IsRecommended { get; set; }

        public int SortOrder { get; set; }

        [StringLength(50)]
        public string Network { get; set; } = "Polygon";

        [Range(1, int.MaxValue)]
        public int ChainId { get; set; } = 137; // Polygon mainnet

        [StringLength(200)]
        public string? RouterAddress { get; set; }

        [StringLength(200)]
        public string? FactoryAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
