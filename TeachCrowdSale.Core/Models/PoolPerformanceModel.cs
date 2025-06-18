using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Staking
{
    /// <summary>
    /// Pool performance model
    /// </summary>
    public class PoolPerformanceModel
    {
        public int PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal TotalStaked { get; set; }

        [Range(0, 200)]
        public decimal CurrentAPY { get; set; }

        [Range(0, 100)]
        public decimal Utilization { get; set; }

        [Range(0, int.MaxValue)]
        public int ParticipantCount { get; set; }
    }
}
