using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Comprehensive buy/trade page data model
    /// </summary>
    public class BuyTradeDataModel
    {
        public TierDisplayModel CurrentTier { get; set; } = new();
        public List<TierDisplayModel> AllTiers { get; set; } = new();
        public PresaleStatsModel PresaleStats { get; set; } = new();
        public TokenInfoModel TokenInfo { get; set; } = new();
        public ContractInfoModel ContractInfo { get; set; } = new();
        public List<PurchaseOptionModel> PurchaseOptions { get; set; } = new();
        public List<DexIntegrationModel> DexIntegrations { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
    }
}
