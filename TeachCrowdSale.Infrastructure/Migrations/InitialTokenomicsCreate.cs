using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachCrowdSale.Infrastructure.Migrations.Tokenomics
{
    /// <inheritdoc />
    public partial class InitialTokenomicsCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Token Metrics Tables
            migrationBuilder.CreateTable(
                name: "TokenMetricsSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    MarketCap = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Volume24h = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceChange24h = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    PriceChangePercent24h = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    High24h = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Low24h = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    TotalSupply = table.Column<long>(type: "bigint", nullable: false),
                    CirculatingSupply = table.Column<long>(type: "bigint", nullable: false),
                    HoldersCount = table.Column<int>(type: "int", nullable: false),
                    TotalValueLocked = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenMetricsSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Pair = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VolumeHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Volume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    VolumeUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Timeframe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolumeHistory", x => x.Id);
                });

            // Supply Management Tables
            migrationBuilder.CreateTable(
                name: "SupplySnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalSupply = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    CirculatingSupply = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    LockedSupply = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    BurnedSupply = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    CirculatingPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LockedPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BurnedPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplySnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TokenAmount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UnlockDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VestingMonths = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenAllocations", x => x.Id);
                });

            // Vesting Tables
            migrationBuilder.CreateTable(
                name: "VestingCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalTokens = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    TgePercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    VestingMonths = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VestingCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VestingMilestones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokensUnlocked = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    CumulativeUnlocked = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    PercentageUnlocked = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsExecuted = table.Column<bool>(type: "bit", nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VestingMilestones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VestingMilestones_VestingCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "VestingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VestingEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnlockDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokensUnlocked = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: true),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VestingEvents", x => x.Id);
                });

            // Treasury Tables
            migrationBuilder.CreateTable(
                name: "TreasurySnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OperationalRunwayYears = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    MonthlyBurnRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SafetyFundValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StabilityFundValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasurySnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TreasuryAllocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SnapshotId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Purpose = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MonthlyUtilization = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProjectedDuration = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasuryAllocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreasuryAllocations_TreasurySnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "TreasurySnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreasuryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreasuryTransactions", x => x.Id);
                });

            // Burn Management Tables
            migrationBuilder.CreateTable(
                name: "BurnEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Mechanism = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    UsdValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BurnEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BurnMechanisms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TriggerPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    HistoricalBurns = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastTriggered = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BurnMechanisms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BurnSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalBurned = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    BurnedPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BurnedLast30Days = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    BurnRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EstimatedAnnualBurn = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    LastBurnDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BurnSnapshots", x => x.Id);
                });

            // Utility & Governance Tables
            migrationBuilder.CreateTable(
                name: "UtilityMetricsSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalUtilityVolume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActiveUtilities = table.Column<int>(type: "int", nullable: false),
                    MonthlyGrowthRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    UniqueUsers = table.Column<long>(type: "bigint", nullable: false),
                    AverageTransactionValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UtilityMetricsSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceProposals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VotesFor = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    VotesAgainst = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    ParticipationRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProposerAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceProposals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GovernanceVotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProposalId = table.Column<int>(type: "int", nullable: false),
                    VoterAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    VoteFor = table.Column<bool>(type: "bit", nullable: false),
                    VotingPower = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    VoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GovernanceVotes_GovernanceProposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "GovernanceProposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create Indexes
            migrationBuilder.CreateIndex(
                name: "IX_TokenMetricsSnapshots_Timestamp",
                table: "TokenMetricsSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TokenMetricsSnapshots_Timestamp_IsLatest",
                table: "TokenMetricsSnapshots",
                columns: new[] { "Timestamp", "IsLatest" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_Timestamp",
                table: "PriceHistory",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_Source_Timestamp",
                table: "PriceHistory",
                columns: new[] { "Source", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_VolumeHistory_Timestamp",
                table: "VolumeHistory",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_VolumeHistory_Source_Timestamp",
                table: "VolumeHistory",
                columns: new[] { "Source", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplySnapshots_Timestamp",
                table: "SupplySnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_SupplySnapshots_IsLatest",
                table: "SupplySnapshots",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_TokenAllocations_Category",
                table: "TokenAllocations",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_VestingCategories_Category",
                table: "VestingCategories",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_VestingMilestones_CategoryId_Date",
                table: "VestingMilestones",
                columns: new[] { "CategoryId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_VestingEvents_UnlockDate",
                table: "VestingEvents",
                column: "UnlockDate");

            migrationBuilder.CreateIndex(
                name: "IX_TreasurySnapshots_Timestamp",
                table: "TreasurySnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TreasurySnapshots_IsLatest",
                table: "TreasurySnapshots",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryAllocations_Category",
                table: "TreasuryAllocations",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryAllocations_SnapshotId",
                table: "TreasuryAllocations",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_Timestamp",
                table: "TreasuryTransactions",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_Category",
                table: "TreasuryTransactions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_TransactionHash",
                table: "TreasuryTransactions",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_BurnEvents_Date",
                table: "BurnEvents",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_BurnEvents_Mechanism",
                table: "BurnEvents",
                column: "Mechanism");

            migrationBuilder.CreateIndex(
                name: "IX_BurnEvents_TransactionHash",
                table: "BurnEvents",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_BurnMechanisms_Name",
                table: "BurnMechanisms",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BurnSnapshots_Timestamp",
                table: "BurnSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_BurnSnapshots_IsLatest",
                table: "BurnSnapshots",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityMetricsSnapshots_Timestamp",
                table: "UtilityMetricsSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityMetricsSnapshots_IsLatest",
                table: "UtilityMetricsSnapshots",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceProposals_Status",
                table: "GovernanceProposals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceProposals_StartDate",
                table: "GovernanceProposals",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceProposals_Category",
                table: "GovernanceProposals",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceVotes_ProposalId",
                table: "GovernanceVotes",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceVotes_VoterAddress",
                table: "GovernanceVotes",
                column: "VoterAddress");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceVotes_VoteDate",
                table: "GovernanceVotes",
                column: "VoteDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokenMetricsSnapshots");

            migrationBuilder.DropTable(
                name: "PriceHistory");

            migrationBuilder.DropTable(
                name: "VolumeHistory");

            migrationBuilder.DropTable(
                name: "SupplySnapshots");

            migrationBuilder.DropTable(
                name: "TokenAllocations");

            migrationBuilder.DropTable(
                name: "VestingMilestones");

            migrationBuilder.DropTable(
                name: "VestingEvents");

            migrationBuilder.DropTable(
                name: "VestingCategories");

            migrationBuilder.DropTable(
                name: "TreasuryAllocations");

            migrationBuilder.DropTable(
                name: "TreasuryTransactions");

            migrationBuilder.DropTable(
                name: "TreasurySnapshots");

            migrationBuilder.DropTable(
                name: "BurnEvents");

            migrationBuilder.DropTable(
                name: "BurnMechanisms");

            migrationBuilder.DropTable(
                name: "BurnSnapshots");

            migrationBuilder.DropTable(
                name: "UtilityMetricsSnapshots");

            migrationBuilder.DropTable(
                name: "GovernanceVotes");

            migrationBuilder.DropTable(
                name: "GovernanceProposals");
        }
    }
}