using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Interfaces
{
    public interface IAbiService
    {
        Task<string> GetContractAbiAsync(string contractName);
        Task RefreshAbisAsync();
    }
}
