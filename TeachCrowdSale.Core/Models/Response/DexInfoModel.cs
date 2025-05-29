using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// DEX information model
    /// </summary>
    public class DexInfoModel
    {
        public bool IsLive { get; set; }
        public DateTime LaunchDate { get; set; }
        public List<TradingPairModel> TradingPairs { get; set; } = new();
        public List<ExchangeListingModel> UpcomingListings { get; set; } = new();
    }
}
