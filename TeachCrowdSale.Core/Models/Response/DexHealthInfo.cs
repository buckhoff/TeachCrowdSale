using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Individual DEX health information
    /// </summary>
    public class DexHealthInfo
    {
        public bool IsOnline { get; set; }
        public int ResponseTimeMs { get; set; }
        public string LastError { get; set; } = string.Empty;
        public DateTime LastSuccessfulCall { get; set; }
        public int FailureCount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
