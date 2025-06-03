using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;
using Task = System.Threading.Tasks.Task;

namespace TeachCrowdSale.Core.Interfaces.Repositories
{
    public interface ITransactionRepository
    {
        Task<PurchaseTransaction> AddPurchaseAsync(PurchaseTransaction transaction);
        Task<ClaimTransaction> AddClaimAsync(ClaimTransaction transaction);
        Task<UserBalance?> GetUserBalanceAsync(string walletAddress);
        Task UpdateUserBalanceAsync(UserBalance userBalance);
        Task<List<PurchaseTransaction>> GetUserPurchasesAsync(string walletAddress);
        Task<PurchaseTransaction> UpdatePurchaseAsync(PurchaseTransaction transaction);
        Task<ClaimTransaction> UpdateClaimAsync(ClaimTransaction transaction);
        Task<List<ClaimTransaction>> GetUserClaimsAsync(string walletAddress);
    }
}
