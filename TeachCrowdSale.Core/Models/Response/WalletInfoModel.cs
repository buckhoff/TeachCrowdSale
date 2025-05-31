using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Wallet information model containing user data and claimable tokens
    /// </summary>
    public class WalletInfoModel
    {
        public UserTradeInfoModel? UserInfo { get; set; }
        public ClaimableTokensModel? ClaimableInfo { get; set; }
        public bool IsConnected { get; set; }
    }
}
