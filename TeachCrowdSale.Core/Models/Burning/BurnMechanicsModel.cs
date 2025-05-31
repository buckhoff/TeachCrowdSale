using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Burning
{
    /// <summary>
    /// Burn mechanics model
    /// </summary>
    public class BurnMechanicsModel
    {
        public BurnStatisticsModel Statistics { get; set; } = new();
        public List<BurnMechanismModel> Mechanisms { get; set; } = new();
        public List<BurnEventModel> RecentBurns { get; set; } = new();
        public BurnProjectionModel Projections { get; set; } = new();
    }

}
