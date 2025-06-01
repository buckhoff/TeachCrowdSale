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

        // Home
        public DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }
        public DbSet<ClaimTransaction> ClaimTransactions { get; set; }
        public DbSet<UserBalance> UserBalances { get; set; }


        // Live Metrics
        public DbSet<TokenMetricsSnapshot> TokenMetricsSnapshots { get; set; }
        public DbSet<PriceHistoryEntry> PriceHistory { get; set; }
        public DbSet<VolumeHistoryEntry> VolumeHistory { get; set; }

        // Supply Management
        public DbSet<SupplySnapshot> SupplySnapshots { get; set; }
        public DbSet<TokenAllocation> TokenAllocations { get; set; }

        // Vesting
        public DbSet<VestingCategory> VestingCategories { get; set; }
        public DbSet<VestingMilestone> VestingMilestones { get; set; }
        public DbSet<VestingEvent> VestingEvents { get; set; }

        // Treasury
        public DbSet<TreasurySnapshot> TreasurySnapshots { get; set; }
        public DbSet<TreasuryAllocation> TreasuryAllocations { get; set; }
        public DbSet<TreasuryTransaction> TreasuryTransactions { get; set; }

        // Burn Management
        public DbSet<BurnEvent> BurnEvents { get; set; }
        public DbSet<BurnMechanism> BurnMechanisms { get; set; }
        public DbSet<BurnSnapshot> BurnSnapshots { get; set; }

        // Utility & Governance
        public DbSet<UtilityMetricsSnapshot> UtilityMetricsSnapshots { get; set; }
        public DbSet<GovernanceProposal> GovernanceProposals { get; set; }
        public DbSet<GovernanceVote> GovernanceVotes { get; set; }

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

            // TokenMetricsSnapshot
            modelBuilder.Entity<TokenMetricsSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CurrentPrice).HasPrecision(18, 8);
                entity.Property(e => e.MarketCap).HasPrecision(18, 2);
                entity.Property(e => e.Volume24h).HasPrecision(18, 2);
                entity.Property(e => e.PriceChange24h).HasPrecision(18, 8);
                entity.Property(e => e.TotalValueLocked).HasPrecision(18, 2);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.Timestamp, e.IsLatest });
            });

            modelBuilder.Entity<PriceHistoryEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(18, 8);
                entity.Property(e => e.Volume).HasPrecision(18, 2);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.Source, e.Timestamp });
            });

            modelBuilder.Entity<VolumeHistoryEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Volume).HasPrecision(18, 2);
                entity.Property(e => e.VolumeUsd).HasPrecision(18, 2);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.Source, e.Timestamp });
            });

            // Supply Management
            modelBuilder.Entity<SupplySnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalSupply).HasPrecision(18, 0);
                entity.Property(e => e.CirculatingSupply).HasPrecision(18, 0);
                entity.Property(e => e.LockedSupply).HasPrecision(18, 0);
                entity.Property(e => e.BurnedSupply).HasPrecision(18, 0);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.IsLatest);
            });

            modelBuilder.Entity<TokenAllocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenAmount).HasPrecision(18, 0);
                entity.Property(e => e.Percentage).HasPrecision(5, 2);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.HasIndex(e => e.Category);
            });

            // Vesting
            modelBuilder.Entity<VestingCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.TotalTokens).HasPrecision(18, 0);
                entity.Property(e => e.TgePercentage).HasPrecision(5, 2);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.HasIndex(e => e.Category);
            });

            modelBuilder.Entity<VestingMilestone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokensUnlocked).HasPrecision(18, 0);
                entity.Property(e => e.CumulativeUnlocked).HasPrecision(18, 0);
                entity.Property(e => e.PercentageUnlocked).HasPrecision(5, 2);
                entity.Property(e => e.Amount).HasPrecision(32,18);
                entity.HasOne<VestingCategory>()
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.CategoryId, e.Date });
            });

            modelBuilder.Entity<VestingEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokensUnlocked).HasPrecision(18, 0);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.HasIndex(e => e.UnlockDate);
                entity.HasIndex(e => e.TransactionHash);
            });

            // Treasury
            modelBuilder.Entity<TreasurySnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalValue).HasPrecision(18, 2);
                entity.Property(e => e.OperationalRunwayYears).HasPrecision(8, 2);
                entity.Property(e => e.MonthlyBurnRate).HasPrecision(18, 2);
                entity.Property(e => e.SafetyFundValue).HasPrecision(18, 2);
                entity.Property(e => e.StabilityFundValue).HasPrecision(18, 2);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.IsLatest);
            });

            modelBuilder.Entity<TreasuryAllocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Value).HasPrecision(18, 2);
                entity.Property(e => e.Percentage).HasPrecision(5, 2);
                entity.Property(e => e.Purpose).HasMaxLength(500);
                entity.Property(e => e.MonthlyUtilization).HasPrecision(18, 2);
                entity.Property(e => e.ProjectedDuration).HasPrecision(8, 2);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.SnapshotId);
            });

            modelBuilder.Entity<TreasuryTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.TransactionHash);
            });

            // Burn Management
            modelBuilder.Entity<BurnEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 0);
                entity.Property(e => e.Mechanism).HasMaxLength(100);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.Property(e => e.UsdValue).HasPrecision(18, 2);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Mechanism);
                entity.HasIndex(e => e.TransactionHash);
            });

            modelBuilder.Entity<BurnMechanism>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TriggerPercentage).HasPrecision(5, 2);
                entity.Property(e => e.Frequency).HasMaxLength(50);
                entity.Property(e => e.HistoricalBurns).HasPrecision(18, 0);
                entity.Property(e => e.Icon).HasMaxLength(10);
                entity.HasIndex(e => e.Name);
            });

            modelBuilder.Entity<BurnSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalBurned).HasPrecision(18, 0);
                entity.Property(e => e.BurnedPercentage).HasPrecision(5, 2);
                entity.Property(e => e.BurnedLast30Days).HasPrecision(18, 0);
                entity.Property(e => e.BurnRate).HasPrecision(5, 2);
                entity.Property(e => e.EstimatedAnnualBurn).HasPrecision(18, 0);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.IsLatest);
            });

            // Utility & Governance
            modelBuilder.Entity<UtilityMetricsSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalUtilityVolume).HasPrecision(18, 2);
                entity.Property(e => e.MonthlyGrowthRate).HasPrecision(5, 2);
                entity.Property(e => e.AverageTransactionValue).HasPrecision(18, 2);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.IsLatest);
            });

            modelBuilder.Entity<GovernanceProposal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.VotesFor).HasPrecision(18, 0);
                entity.Property(e => e.VotesAgainst).HasPrecision(18, 0);
                entity.Property(e => e.ParticipationRate).HasPrecision(5, 2);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.ProposerAddress).HasMaxLength(42);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.Category);
            });

            modelBuilder.Entity<GovernanceVote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VoterAddress).HasMaxLength(42);
                entity.Property(e => e.VotingPower).HasPrecision(18, 0);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.HasOne<GovernanceProposal>()
                    .WithMany()
                    .HasForeignKey(e => e.ProposalId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.ProposalId);
                entity.HasIndex(e => e.VoterAddress);
                entity.HasIndex(e => e.VoteDate);
            });
        }
    }

}
