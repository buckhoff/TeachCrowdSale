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

        public DbSet<AnalyticsSnapshot> AnalyticsSnapshots { get; set; }
        public DbSet<TierSnapshot> TierSnapshots { get; set; }
        public DbSet<DailyAnalytics> DailyAnalytics { get; set; }
        public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureTransactionEntities(modelBuilder);
            ConfigureMetricsEntities(modelBuilder);
            ConfigureSupplyEntities(modelBuilder);
            ConfigureVestingEntities(modelBuilder);
            ConfigureTreasuryEntities(modelBuilder);
            ConfigureBurnEntities(modelBuilder);
            ConfigureUtilityGovernanceEntities(modelBuilder);
            ConfigureAnalyticsEntities(modelBuilder);
        }

        private void ConfigureTransactionEntities(ModelBuilder modelBuilder)
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
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.TierId);
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
                entity.HasIndex(e => e.CreatedAt);
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
                entity.HasIndex(e => e.LastUpdated);

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

        private void ConfigureMetricsEntities(ModelBuilder modelBuilder)
        {
            // TokenMetricsSnapshot
            modelBuilder.Entity<TokenMetricsSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CurrentPrice).HasPrecision(18, 8);
                entity.Property(e => e.MarketCap).HasPrecision(18, 2);
                entity.Property(e => e.Volume24h).HasPrecision(18, 2);
                entity.Property(e => e.PriceChange24h).HasPrecision(18, 8);
                entity.Property(e => e.TotalValueLocked).HasPrecision(18, 2);
                entity.Property(e => e.TotalSupply).HasPrecision(18, 0);
                entity.Property(e => e.CirculatingSupply).HasPrecision(18, 0);
                entity.Property(e => e.Source).HasMaxLength(50);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.Timestamp, e.IsLatest });
                entity.HasIndex(e => e.Source);
            });

            // PriceHistoryEntry
            modelBuilder.Entity<PriceHistoryEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Price).HasPrecision(18, 8);
                entity.Property(e => e.Volume).HasPrecision(18, 2);
                entity.Property(e => e.Source).HasMaxLength(50);
                entity.Property(e => e.Pair).HasMaxLength(20);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.Source, e.Timestamp });
                entity.HasIndex(e => new { e.Pair, e.Timestamp });
            });

            // VolumeHistoryEntry
            modelBuilder.Entity<VolumeHistoryEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Volume).HasPrecision(18, 2);
                entity.Property(e => e.VolumeUsd).HasPrecision(18, 2);
                entity.Property(e => e.Source).HasMaxLength(50);
                entity.Property(e => e.Timeframe).HasMaxLength(10);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.Source, e.Timestamp });
                entity.HasIndex(e => new { e.Timeframe, e.Timestamp });
            });
        }

        private void ConfigureSupplyEntities(ModelBuilder modelBuilder)
        {
            // SupplySnapshot
            modelBuilder.Entity<SupplySnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalSupply).HasPrecision(18, 0);
                entity.Property(e => e.CirculatingSupply).HasPrecision(18, 0);
                entity.Property(e => e.LockedSupply).HasPrecision(18, 0);
                entity.Property(e => e.BurnedSupply).HasPrecision(18, 0);
                entity.Property(e => e.CirculatingPercent).HasPrecision(5, 2);
                entity.Property(e => e.LockedPercent).HasPrecision(5, 2);
                entity.Property(e => e.BurnedPercent).HasPrecision(5, 2);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.IsLatest);
            });

            // TokenAllocation
            modelBuilder.Entity<TokenAllocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenAmount).HasPrecision(18, 0);
                entity.Property(e => e.Percentage).HasPrecision(5, 2);
                entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.UnlockDate);
            });
        }

        private void ConfigureVestingEntities(ModelBuilder modelBuilder)
        {
            // VestingCategory
            modelBuilder.Entity<VestingCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TotalTokens).HasPrecision(18, 0);
                entity.Property(e => e.TgePercentage).HasPrecision(5, 2);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.HasIndex(e => e.Category).IsUnique();
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.IsActive);
            });

            // VestingMilestone
            modelBuilder.Entity<VestingMilestone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokensUnlocked).HasPrecision(18, 0);
                entity.Property(e => e.CumulativeUnlocked).HasPrecision(18, 0);
                entity.Property(e => e.PercentageUnlocked).HasPrecision(5, 2);
                entity.Property(e => e.Amount).HasPrecision(32, 18);
                entity.Property(e => e.FormattedDate).HasMaxLength(50);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.HasOne<VestingCategory>()
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.CategoryId, e.Date });
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.IsExecuted);
            });

            // VestingEvent
            modelBuilder.Entity<VestingEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokensUnlocked).HasPrecision(18, 0);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.HasIndex(e => e.UnlockDate);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.TransactionHash);
                entity.HasIndex(e => e.IsProcessed);
            });
        }

        private void ConfigureTreasuryEntities(ModelBuilder modelBuilder)
        {
            // TreasurySnapshot
            modelBuilder.Entity<TreasurySnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalValue).HasPrecision(18, 2);
                entity.Property(e => e.OperationalRunwayYears).HasPrecision(8, 2);
                entity.Property(e => e.MonthlyBurnRate).HasPrecision(18, 2);
                entity.Property(e => e.SafetyFundValue).HasPrecision(18, 2);
                entity.Property(e => e.StabilityFundValue).HasPrecision(18, 2);
                entity.Property(e => e.Source).HasMaxLength(50);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.IsLatest);
                entity.HasIndex(e => e.Source);
            });

            // TreasuryAllocation
            modelBuilder.Entity<TreasuryAllocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Value).HasPrecision(18, 2);
                entity.Property(e => e.Percentage).HasPrecision(5, 2);
                entity.Property(e => e.Purpose).HasMaxLength(500);
                entity.Property(e => e.MonthlyUtilization).HasPrecision(18, 2);
                entity.Property(e => e.ProjectedDuration).HasPrecision(8, 2);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.SnapshotId);
            });

            // TreasuryTransaction
            modelBuilder.Entity<TreasuryTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TransactionType).HasMaxLength(20).IsRequired();
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.TransactionType);
                entity.HasIndex(e => e.TransactionHash);
                entity.HasIndex(e => e.IsVerified);
            });
        }

        private void ConfigureBurnEntities(ModelBuilder modelBuilder)
        {
            // BurnEvent
            modelBuilder.Entity<BurnEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasPrecision(18, 0);
                entity.Property(e => e.Mechanism).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TransactionHash).HasMaxLength(66).IsRequired();
                entity.Property(e => e.UsdValue).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Mechanism);
                entity.HasIndex(e => e.TransactionHash).IsUnique();
                entity.HasIndex(e => e.IsVerified);
            });

            // BurnMechanism
            modelBuilder.Entity<BurnMechanism>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TriggerPercentage).HasPrecision(5, 2);
                entity.Property(e => e.Frequency).HasMaxLength(50);
                entity.Property(e => e.HistoricalBurns).HasPrecision(18, 0);
                entity.Property(e => e.Icon).HasMaxLength(10);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // BurnSnapshot
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
                entity.HasIndex(e => e.LastBurnDate);
            });
        }

        private void ConfigureUtilityGovernanceEntities(ModelBuilder modelBuilder)
        {
            // UtilityMetricsSnapshot
            modelBuilder.Entity<UtilityMetricsSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalUtilityVolume).HasPrecision(18, 2);
                entity.Property(e => e.MonthlyGrowthRate).HasPrecision(5, 2);
                entity.Property(e => e.AverageTransactionValue).HasPrecision(18, 2);
                entity.Property(e => e.Source).HasMaxLength(50);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.IsLatest);
                entity.HasIndex(e => e.Source);
            });

            // GovernanceProposal
            modelBuilder.Entity<GovernanceProposal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.VotesFor).HasPrecision(18, 0);
                entity.Property(e => e.VotesAgainst).HasPrecision(18, 0);
                entity.Property(e => e.ParticipationRate).HasPrecision(5, 2);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.ProposerAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.ProposerAddress);
            });

            // GovernanceVote
            modelBuilder.Entity<GovernanceVote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VoterAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.VotingPower).HasPrecision(18, 0);
                entity.Property(e => e.TransactionHash).HasMaxLength(66).IsRequired();
                entity.HasOne<GovernanceProposal>()
                    .WithMany()
                    .HasForeignKey(e => e.ProposalId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.ProposalId);
                entity.HasIndex(e => e.VoterAddress);
                entity.HasIndex(e => e.VoteDate);
                entity.HasIndex(e => new { e.ProposalId, e.VoterAddress }).IsUnique();
            });
        }

        private void ConfigureAnalyticsEntities(ModelBuilder modelBuilder)
        {
            // AnalyticsSnapshot
            modelBuilder.Entity<AnalyticsSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokenPrice).HasPrecision(18, 8);
                entity.Property(e => e.MarketCap).HasPrecision(18, 2);
                entity.Property(e => e.Volume24h).HasPrecision(18, 2);
                entity.Property(e => e.PriceChange24h).HasPrecision(18, 8);
                entity.Property(e => e.TotalRaised).HasPrecision(18, 2);
                entity.Property(e => e.TokensSold).HasPrecision(18, 0);
                entity.Property(e => e.TotalValueLocked).HasPrecision(18, 2);
                entity.Property(e => e.StakedTokens).HasPrecision(18, 0);
                entity.Property(e => e.RewardsDistributed).HasPrecision(18, 8);
                entity.Property(e => e.TreasuryBalance).HasPrecision(18, 2);
                entity.Property(e => e.StabilityFundBalance).HasPrecision(18, 2);
                entity.Property(e => e.BurnedTokens).HasPrecision(18, 0);

                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.ActiveTierId);
                entity.HasIndex(e => new { e.Date, e.Timestamp });

                entity.HasMany(e => e.TierSnapshots)
                    .WithOne(t => t.AnalyticsSnapshot)
                    .HasForeignKey(t => t.AnalyticsSnapshotId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TierSnapshot
            modelBuilder.Entity<TierSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TierName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Price).HasPrecision(18, 8);
                entity.Property(e => e.Allocation).HasPrecision(18, 0);
                entity.Property(e => e.Sold).HasPrecision(18, 0);
                entity.Property(e => e.SoldChange24h).HasPrecision(18, 0);

                entity.HasIndex(e => e.TierId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.TierId, e.Timestamp });
                entity.HasIndex(e => e.IsActive);
            });

            // DailyAnalytics
            modelBuilder.Entity<DailyAnalytics>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DailyVolume).HasPrecision(18, 2);
                entity.Property(e => e.DailyTokensSold).HasPrecision(18, 0);
                entity.Property(e => e.DailyUsdRaised).HasPrecision(18, 2);
                entity.Property(e => e.OpenPrice).HasPrecision(18, 8);
                entity.Property(e => e.ClosePrice).HasPrecision(18, 8);
                entity.Property(e => e.HighPrice).HasPrecision(18, 8);
                entity.Property(e => e.LowPrice).HasPrecision(18, 8);
                entity.Property(e => e.DailyRewardsDistributed).HasPrecision(18, 8);
                entity.Property(e => e.EducationFundingAmount).HasPrecision(18, 2);

                entity.HasIndex(e => e.Date).IsUnique();
                entity.HasIndex(e => e.Date);
            });

            // PerformanceMetric
            modelBuilder.Entity<PerformanceMetric>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MetricName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Value).HasPrecision(18, 8);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.PreviousValue).HasPrecision(18, 8);
                entity.Property(e => e.ChangePercentage).HasPrecision(8, 4);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasIndex(e => e.MetricName);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => new { e.MetricName, e.Timestamp });
                entity.HasIndex(e => new { e.Category, e.Timestamp });
                entity.HasIndex(e => e.IsPublic);
            });
        }
    }

}
