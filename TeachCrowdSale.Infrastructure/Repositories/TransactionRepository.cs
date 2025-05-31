using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;
using TeachCrowdSale.Core.Interfaces.Repositories;
using TeachCrowdSale.Infrastructure.Data.Context;

namespace TeachCrowdSale.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly TeachCrowdSaleDbContext _context;

        public TransactionRepository(TeachCrowdSaleDbContext context)
        {
            _context = context;
        }

        public async Task<PurchaseTransaction> AddPurchaseAsync(PurchaseTransaction transaction)
        {
            _context.PurchaseTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<ClaimTransaction> AddClaimAsync(ClaimTransaction transaction)
        {
            _context.ClaimTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<UserBalance?> GetUserBalanceAsync(string walletAddress)
        {
            return await _context.UserBalances
                .Include(u => u.Purchases)
                .Include(u => u.Claims)
                .FirstOrDefaultAsync(u => u.WalletAddress == walletAddress);
        }

        public async Task UpdateUserBalanceAsync(UserBalance userBalance)
        {
            _context.UserBalances.Update(userBalance);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PurchaseTransaction>> GetUserPurchasesAsync(string walletAddress)
        {
            return await _context.PurchaseTransactions
                .Where(p => p.WalletAddress == walletAddress)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        public async Task<PurchaseTransaction> UpdatePurchaseAsync(PurchaseTransaction transaction)
        {
            _context.PurchaseTransactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<ClaimTransaction> UpdateClaimAsync(ClaimTransaction transaction)
        {
            _context.ClaimTransactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<ClaimTransaction>> GetUserClaimsAsync(string walletAddress)
        {
            return await _context.ClaimTransactions
                .Where(c => c.WalletAddress == walletAddress)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

    }
}
