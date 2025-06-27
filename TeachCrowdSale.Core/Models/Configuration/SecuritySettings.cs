using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Configuration
{
    public class SecuritySettings
    {
        public string MaxTransactionValue { get; set; } = string.Empty;
        public string RequireKYCAbove { get; set; } = string.Empty;
        public bool AMLCheckEnabled { get; set; } = true;
        public bool GeoblockingEnabled { get; set; } = false;
        public List<string> BlockedCountries { get; set; } = new();
        public bool TwoFactorAuthRequired { get; set; } = false;
    }
}
