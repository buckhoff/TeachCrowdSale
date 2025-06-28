using Microsoft.EntityFrameworkCore;
using TeachCrowdSale.Core.Data.Entities;

namespace TeachCrowdSale.Infrastructure.Data.Context
{
    public class TeachCrowdSaleDbContext : DbContext
    {
        public TeachCrowdSaleDbContext(DbContextOptions<TeachCrowdSaleDbContext> options) : base(options) { }

        // Basic Transaction Entities (3)
        public DbSet<PurchaseTransaction> PurchaseTransactions { get; set; }
        public DbSet<ClaimTransaction> ClaimTransactions { get; set; }
        public DbSet<UserBalance> UserBalances { get; set; }

        // Live Metrics Entities (3)
        public DbSet<TokenMetricsSnapshot> TokenMetricsSnapshots { get; set; }
        public DbSet<PriceHistoryEntry> PriceHistory { get; set; }
        public DbSet<VolumeHistoryEntry> VolumeHistory { get; set; }

        // Supply Management Entities (2)
        public DbSet<SupplySnapshot> SupplySnapshots { get; set; }
        public DbSet<TokenAllocation> TokenAllocations { get; set; }

        // Vesting Entities (3)
        public DbSet<VestingCategory> VestingCategories { get; set; }
        public DbSet<VestingMilestone> VestingMilestones { get; set; }
        public DbSet<VestingEvent> VestingEvents { get; set; }

        // Treasury Entities (3)
        public DbSet<TreasurySnapshot> TreasurySnapshots { get; set; }
        public DbSet<TreasuryAllocation> TreasuryAllocations { get; set; }
        public DbSet<TreasuryTransaction> TreasuryTransactions { get; set; }

        // Burn Management Entities (3)
        public DbSet<BurnEvent> BurnEvents { get; set; }
        public DbSet<BurnMechanism> BurnMechanisms { get; set; }
        public DbSet<BurnSnapshot> BurnSnapshots { get; set; }

        // Utility & Governance Entities (3)
        public DbSet<UtilityMetricsSnapshot> UtilityMetricsSnapshots { get; set; }
        public DbSet<GovernanceProposal> GovernanceProposals { get; set; }
        public DbSet<GovernanceVote> GovernanceVotes { get; set; }

        // Analytics Entities (4)
        public DbSet<AnalyticsSnapshot> AnalyticsSnapshots { get; set; }
        public DbSet<TierSnapshot> TierSnapshots { get; set; }
        public DbSet<DailyAnalytics> DailyAnalytics { get; set; }
        public DbSet<PerformanceMetric> PerformanceMetrics { get; set; }

        // Staking Entities (6)
        public DbSet<StakingPool> StakingPools { get; set; }
        public DbSet<UserStake> UserStakes { get; set; }
        public DbSet<StakingRewardClaim> StakingRewardClaims { get; set; }
        public DbSet<SchoolBeneficiary> SchoolBeneficiaries { get; set; }
        public DbSet<UserStakingBeneficiary> UserStakingBeneficiaries { get; set; }
        public DbSet<SchoolRewardDistribution> SchoolRewardDistributions { get; set; }

        // Liquidity Entities (5)
        public DbSet<LiquidityPool> LiquidityPools { get; set; }
        public DbSet<UserLiquidityPosition> UserLiquidityPositions { get; set; }
        public DbSet<LiquidityTransaction> LiquidityTransactions { get; set; }
        public DbSet<LiquidityPoolSnapshot> LiquidityPoolSnapshots { get; set; }
        public DbSet<DexConfiguration> DexConfigurations { get; set; }

        //Pencil Impact Home (5)
        public DbSet<PlatformWaitlist> PlatformWaitlists { get; set; }
        public DbSet<PencilDrive> PencilDrives { get; set; }
        public DbSet<DemoProject> DemoProjects { get; set; }
        public DbSet<PlatformAnalytics> PlatformAnalytics { get; set; }
        public DbSet<PencilDriveImpactStory> PencilDriveImpactStories { get; set; }


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
            ConfigureStakingEntities(modelBuilder);
            ConfigureLiquidityEntities(modelBuilder);
            ConfigurePencilImpactEntities(modelBuilder);
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
            // TokenMetricsSnapshot - Based on actual entity properties
            modelBuilder.Entity<TokenMetricsSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CurrentPrice).HasPrecision(18, 8);
                entity.Property(e => e.MarketCap).HasPrecision(18, 2);
                entity.Property(e => e.Volume24h).HasPrecision(18, 2);
                entity.Property(e => e.PriceChange24h).HasPrecision(18, 8);
                entity.Property(e => e.PriceChangePercent24h).HasPrecision(18, 8); // ADDED
                entity.Property(e => e.High24h).HasPrecision(18, 8); // ADDED
                entity.Property(e => e.Low24h).HasPrecision(18, 8); // ADDED
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

            // VestingMilestone - Based on actual entity with long properties
            modelBuilder.Entity<VestingMilestone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TokensUnlocked).HasColumnType("bigint"); // long
                entity.Property(e => e.CumulativeUnlocked).HasColumnType("bigint"); // long
                entity.Property(e => e.PercentageUnlocked).HasPrecision(5, 2);
                entity.Property(e => e.Amount).HasPrecision(32, 18);
                entity.Property(e => e.FormattedDate).HasMaxLength(50);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
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
            // BurnEvent - Based on actual entity with long Amount
            modelBuilder.Entity<BurnEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("bigint"); // long in C#
                entity.Property(e => e.Mechanism).HasMaxLength(100).IsRequired();
                entity.Property(e => e.TransactionHash).HasMaxLength(66).IsRequired();
                entity.Property(e => e.UsdValue).HasPrecision(18, 2);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasIndex(e => e.Date);
                entity.HasIndex(e => e.Mechanism);
                entity.HasIndex(e => e.TransactionHash).IsUnique();
                entity.HasIndex(e => e.IsVerified);
            });

            // BurnMechanism - COMPLETE entity configuration
            modelBuilder.Entity<BurnMechanism>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.TriggerPercentage).HasPrecision(5, 2);
                entity.Property(e => e.Frequency).HasMaxLength(50);
                entity.Property(e => e.HistoricalBurns).HasPrecision(18, 0);
                entity.Property(e => e.Icon).HasMaxLength(10);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
            });

            // BurnSnapshot - COMPLETE entity configuration
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
                entity.HasIndex(e => e.ProposalId);
                entity.HasIndex(e => e.VoterAddress);
                entity.HasIndex(e => e.VoteDate);
                entity.HasIndex(e => new { e.ProposalId, e.VoterAddress }).IsUnique();
            });
        }

        private void ConfigureAnalyticsEntities(ModelBuilder modelBuilder)
        {
            // AnalyticsSnapshot - Based on actual entity
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
                entity.HasIndex(e => e.ActiveTierId);

                entity.HasMany(e => e.TierSnapshots)
                    .WithOne(t => t.AnalyticsSnapshot)
                    .HasForeignKey(t => t.AnalyticsSnapshotId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // TierSnapshot - Based on actual entity
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
                entity.HasIndex(e => new { e.MetricName, e.Timestamp });
                entity.HasIndex(e => new { e.Category, e.Timestamp });
                entity.HasIndex(e => e.IsPublic);
            });
        }

        private void ConfigureStakingEntities(ModelBuilder modelBuilder)
        {
            // StakingPool - Based on actual entity
            modelBuilder.Entity<StakingPool>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.MinStakeAmount).HasPrecision(18, 8);
                entity.Property(e => e.MaxStakeAmount).HasPrecision(18, 8);
                entity.Property(e => e.BaseAPY).HasPrecision(8, 4);
                entity.Property(e => e.BonusAPY).HasPrecision(8, 4);
                entity.Property(e => e.TotalStaked).HasPrecision(18, 8);
                entity.Property(e => e.MaxPoolSize).HasPrecision(18, 8);
                entity.Property(e => e.EarlyWithdrawalPenalty).HasPrecision(5, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.LockPeriodDays);
            });

            // UserStake - Based on actual entity
            modelBuilder.Entity<UserStake>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.StakedAmount).HasPrecision(18, 8);
                entity.Property(e => e.AccruedRewards).HasPrecision(18, 8);
                entity.Property(e => e.ClaimedRewards).HasPrecision(18, 8);
                entity.Property(e => e.StakeTransactionHash).HasMaxLength(66);
                entity.Property(e => e.UnstakeTransactionHash).HasMaxLength(66);
                entity.HasIndex(e => e.WalletAddress);
                entity.HasIndex(e => e.StakingPoolId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.StakeDate);

                entity.HasOne(e => e.StakingPool)
                    .WithMany(p => p.UserStakes)
                    .HasForeignKey(e => e.StakingPoolId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // StakingRewardClaim - Based on actual entity
            modelBuilder.Entity<StakingRewardClaim>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClaimedAmount).HasPrecision(18, 8);
                entity.Property(e => e.TransactionHash).HasMaxLength(66).IsRequired();
                entity.HasIndex(e => e.UserStakeId);
                entity.HasIndex(e => e.ClaimDate);
                entity.HasIndex(e => e.TransactionHash);

                entity.HasOne(e => e.UserStake)
                    .WithMany(s => s.RewardClaims)
                    .HasForeignKey(e => e.UserStakeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // SchoolBeneficiary - COMPLETE entity configuration with ALL properties
            modelBuilder.Entity<SchoolBeneficiary>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.WalletAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.ContactEmail).HasMaxLength(255);
                entity.Property(e => e.Website).HasMaxLength(500);
                entity.Property(e => e.LogoUrl).HasMaxLength(500);
                entity.Property(e => e.TotalReceived).HasPrecision(18, 8);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.WalletAddress).IsUnique();
                entity.HasIndex(e => e.Country);
                entity.HasIndex(e => e.State);
                entity.HasIndex(e => e.IsVerified);
                entity.HasIndex(e => e.IsActive);
            });

            // UserStakingBeneficiary - COMPLETE entity configuration with ALL 6 properties
            modelBuilder.Entity<UserStakingBeneficiary>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.TotalDonated).HasPrecision(18, 8);
                entity.Property(e => e.SelectedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.WalletAddress);
                entity.HasIndex(e => e.SchoolBeneficiaryId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.WalletAddress, e.IsActive })
                    .IsUnique()
                    .HasFilter("[IsActive] = 1");

                entity.HasOne(e => e.SchoolBeneficiary)
                    .WithMany(s => s.UserStakes)
                    .HasForeignKey(e => e.SchoolBeneficiaryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // SchoolRewardDistribution - COMPLETE entity configuration with ALL 7 properties
            modelBuilder.Entity<SchoolRewardDistribution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StakerAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.Amount).HasPrecision(18, 8);
                entity.Property(e => e.TransactionHash).HasMaxLength(66);
                entity.Property(e => e.DistributionDate).HasDefaultValueSql("GETUTCDATE()");
                entity.HasIndex(e => e.SchoolBeneficiaryId);
                entity.HasIndex(e => e.StakerAddress);
                entity.HasIndex(e => e.DistributionDate);
                entity.HasIndex(e => e.TransactionHash);
                entity.HasIndex(e => e.Status);

                entity.HasOne(e => e.SchoolBeneficiary)
                    .WithMany()
                    .HasForeignKey(e => e.SchoolBeneficiaryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureLiquidityEntities(ModelBuilder modelBuilder)
        {
            // LiquidityPool
            modelBuilder.Entity<LiquidityPool>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
                entity.Property(e => e.DexName).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TokenPair).HasMaxLength(20).IsRequired();
                entity.Property(e => e.PoolAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.Token0Address).HasMaxLength(42).IsRequired();
                entity.Property(e => e.Token1Address).HasMaxLength(42).IsRequired();
                entity.Property(e => e.Token0Symbol).HasMaxLength(10).IsRequired();
                entity.Property(e => e.Token1Symbol).HasMaxLength(10).IsRequired();
                entity.Property(e => e.TotalValueLocked).HasPrecision(18, 2);
                entity.Property(e => e.Volume24h).HasPrecision(18, 2);
                entity.Property(e => e.Volume7d).HasPrecision(18, 2);
                entity.Property(e => e.FeePercentage).HasPrecision(5, 3);
                entity.Property(e => e.APY).HasPrecision(8, 4);
                entity.Property(e => e.APR).HasPrecision(8, 4);
                entity.Property(e => e.Token0Reserve).HasPrecision(18, 8);
                entity.Property(e => e.Token1Reserve).HasPrecision(18, 8);
                entity.Property(e => e.CurrentPrice).HasPrecision(18, 8);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.DexUrl).HasMaxLength(200);
                entity.Property(e => e.AnalyticsUrl).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastSyncAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.PoolAddress).IsUnique();
                entity.HasIndex(e => e.DexName);
                entity.HasIndex(e => e.TokenPair);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.TotalValueLocked);
                entity.HasIndex(e => e.LastSyncAt);

                // Navigation properties
                entity.HasMany(e => e.UserPositions)
                    .WithOne(p => p.LiquidityPool)
                    .HasForeignKey(p => p.LiquidityPoolId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Snapshots)
                    .WithOne(s => s.LiquidityPool)
                    .HasForeignKey(s => s.LiquidityPoolId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserLiquidityPosition
            modelBuilder.Entity<UserLiquidityPosition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.LpTokenAmount).HasPrecision(18, 8);
                entity.Property(e => e.Token0Amount).HasPrecision(18, 8);
                entity.Property(e => e.Token1Amount).HasPrecision(18, 8);
                entity.Property(e => e.InitialValueUsd).HasPrecision(18, 2);
                entity.Property(e => e.CurrentValueUsd).HasPrecision(18, 2);
                entity.Property(e => e.FeesEarnedUsd).HasPrecision(18, 6);
                entity.Property(e => e.ImpermanentLoss).HasPrecision(18, 6);
                entity.Property(e => e.NetPnL).HasPrecision(18, 6);
                entity.Property(e => e.AddTransactionHash).HasMaxLength(66);
                entity.Property(e => e.RemoveTransactionHash).HasMaxLength(66);
                entity.Property(e => e.AddedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.WalletAddress);
                entity.HasIndex(e => e.LiquidityPoolId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.AddedAt);
                entity.HasIndex(e => new { e.WalletAddress, e.IsActive });

                entity.HasMany(e => e.Transactions)
                    .WithOne(t => t.UserLiquidityPosition)
                    .HasForeignKey(t => t.UserLiquidityPositionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LiquidityTransaction
            modelBuilder.Entity<LiquidityTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WalletAddress).HasMaxLength(42).IsRequired();
                entity.Property(e => e.TransactionType).HasMaxLength(20).IsRequired();
                entity.Property(e => e.Token0Amount).HasPrecision(18, 8);
                entity.Property(e => e.Token1Amount).HasPrecision(18, 8);
                entity.Property(e => e.LpTokenAmount).HasPrecision(18, 8);
                entity.Property(e => e.ValueUsd).HasPrecision(18, 2);
                entity.Property(e => e.GasFeesUsd).HasPrecision(18, 6);
                entity.Property(e => e.TransactionHash).HasMaxLength(66).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.UserLiquidityPositionId);
                entity.HasIndex(e => e.WalletAddress);
                entity.HasIndex(e => e.TransactionType);
                entity.HasIndex(e => e.TransactionHash);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Status);
            });

            // LiquidityPoolSnapshot - Based on actual entity
            modelBuilder.Entity<LiquidityPoolSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalValueLocked).HasPrecision(18, 2);
                entity.Property(e => e.Volume24h).HasPrecision(18, 2);
                entity.Property(e => e.Token0Reserve).HasPrecision(18, 8);
                entity.Property(e => e.Token1Reserve).HasPrecision(18, 8);
                entity.Property(e => e.Price).HasPrecision(18, 8);
                entity.Property(e => e.APY).HasPrecision(8, 4);
                entity.Property(e => e.APR).HasPrecision(8, 4);
                entity.Property(e => e.Source).HasMaxLength(50);
                entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.LiquidityPoolId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.LiquidityPoolId, e.Timestamp });
                entity.HasIndex(e => new { e.LiquidityPoolId, e.IsLatest });
                entity.HasIndex(e => e.Source);

                entity.HasOne(e => e.LiquidityPool)
                    .WithMany(p => p.Snapshots)
                    .HasForeignKey(e => e.LiquidityPoolId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // DexConfiguration
            modelBuilder.Entity<DexConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
                entity.Property(e => e.DisplayName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.BaseUrl).HasMaxLength(200).IsRequired();
                entity.Property(e => e.ApiUrl).HasMaxLength(200).IsRequired();
                entity.Property(e => e.LogoUrl).HasMaxLength(200).IsRequired();
                entity.Property(e => e.DefaultFeePercentage).HasPrecision(5, 3);
                entity.Property(e => e.Network).HasMaxLength(50).IsRequired();
                entity.Property(e => e.RouterAddress).HasMaxLength(42);
                entity.Property(e => e.FactoryAddress).HasMaxLength(42);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Network);
                entity.HasIndex(e => e.SortOrder);
                entity.HasIndex(e => e.IsRecommended);
            });
        }

        private void ConfigurePencilImpactEntities(ModelBuilder modelBuilder)
        {
            // PlatformWaitlist configuration
            modelBuilder.Entity<PlatformWaitlist>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
                entity.Property(e => e.UserType).HasMaxLength(50).IsRequired();
                entity.Property(e => e.SchoolDistrict).HasMaxLength(200);
                entity.Property(e => e.TeachingSubject).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserType);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.InterestedInTEACHTokens);
            });

            // PencilDrive configuration
            modelBuilder.Entity<PencilDrive>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.PencilGoal).IsRequired();
                entity.Property(e => e.TokensRaised).HasPrecision(18, 8);
                entity.Property(e => e.PartnerName).HasMaxLength(200);
                entity.Property(e => e.PartnerLogoUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(e => e.Year).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.EndDate);
            });

            // DemoProject configuration
            modelBuilder.Entity<DemoProject>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
                entity.Property(e => e.FundingGoal).HasPrecision(10, 2).IsRequired();
                entity.Property(e => e.CurrentFunding).HasPrecision(10, 2);
                entity.Property(e => e.SchoolName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.TeacherName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.State).HasMaxLength(2);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.GradeLevel).HasMaxLength(50);
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.State);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.IsUrgent);
                entity.HasIndex(e => e.Deadline);
                entity.HasIndex(e => e.CreatedAt);
            });

            // PlatformAnalytics configuration
            modelBuilder.Entity<PlatformAnalytics>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PageUrl).HasMaxLength(500).IsRequired();
                entity.Property(e => e.SessionId).HasMaxLength(100);
                entity.Property(e => e.UserAgent).HasMaxLength(1000);
                entity.Property(e => e.ReferrerUrl).HasMaxLength(500);
                entity.Property(e => e.UserIP).HasMaxLength(45);
                entity.Property(e => e.ConversionAction).HasMaxLength(100);
                entity.Property(e => e.ConversionData).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Indexes
                entity.HasIndex(e => e.PageUrl);
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => e.ConversionAction);
                entity.HasIndex(e => e.CreatedAt);
            });

            // PencilDriveImpactStory configuration
            modelBuilder.Entity<PencilDriveImpactStory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SchoolName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.TeacherName).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(2);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.StoryText).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.VideoUrl).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Foreign key relationship
                entity.HasOne(e => e.PencilDrive)
                      .WithMany()
                      .HasForeignKey(e => e.PencilDriveId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.PencilDriveId);
                entity.HasIndex(e => e.State);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}