using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Infrastructure.Data.Context
{
    public class TeachCrowdSaleDbContext : DbContext
    {
        public TeachCrowdSaleDbContext(DbContextOptions<TeachCrowdSaleDbContext> options) : base(options) { }

        public DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }
        public DbSet<ClaimTransaction> ClaimTransactions { get; set; }
        public DbSet<UserBalance> UserBalances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // PurchaseTransaction
            modelBuilder.Entity<PurchaseTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.UsdAmount).HasPrecision(18, 6);
                entity.Property(e => e.TokenAmount).HasPrecision(18, 8);
                entity.Property(e => e.TokenPrice).HasPrecision(18, 8);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.HasIndex(e => e.WalletAddress);
                entity.HasIndex(e => e.TransactionHash);
            });

            // ClaimTransaction
            modelBuilder.Entity<ClaimTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.TokenAmount).HasPrecision(18, 8);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.HasIndex(e => e.WalletAddress);
                entity.HasIndex(e => e.TransactionHash);
            });

            // UserBalance
            modelBuilder.Entity<UserBalance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.TotalPurchased).HasPrecision(18, 8);
                entity.Property(e => e.TotalClaimed).HasPrecision(18, 8);
                entity.Property(e => e.PendingTokens).HasPrecision(18, 8);
                entity.HasIndex(e => e.WalletAddress).IsUnique();

                entity.HasMany(e => e.Purchases)
                    .WithOne()
                    .HasForeignKey("UserBalanceId")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey("UserBalanceId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }

}
