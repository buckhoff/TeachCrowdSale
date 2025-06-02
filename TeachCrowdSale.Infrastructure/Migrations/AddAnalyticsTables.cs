// TeachCrowdSale.Infrastructure/Migrations/AddAnalyticsTables.cs
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachCrowdSale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create AnalyticsSnapshots table
            migrationBuilder.CreateTable(
                name: "AnalyticsSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    MarketCap = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Volume24h = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PriceChange24h = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: false),
                    HoldersCount = table.Column<int>(type: "int", nullable: false),
                    TotalRaised = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TokensSold = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    ParticipantsCount = table.Column<int>(type: "int", nullable: false),
                    ActiveTierId = table.Column<int>(type: "int", nullable: false),
                    TotalValueLocked = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StakedTokens = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    RewardsDistributed = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    ActiveStakers = table.Column<int>(type: "int", nullable: false),
                    TreasuryBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StabilityFundBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BurnedTokens = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    TransactionsCount24h = table.Column<int>(type: "int", nullable: false),
                    UniqueUsers24h = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false, computedColumnSql: "CAST([Timestamp] AS DATE)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsSnapshots", x => x.Id);
                });

            // Create DailyAnalytics table
            migrationBuilder.CreateTable(
                name: "DailyAnalytics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DailyVolume = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DailyTransactions = table.Column<int>(type: "int", nullable: false),
                    NewHolders = table.Column<int>(type: "int", nullable: false),
                    NewParticipants = table.Column<int>(type: "int", nullable: false),
                    DailyTokensSold = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    DailyUsdRaised = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OpenPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    ClosePrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    HighPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    LowPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    DailyRewardsDistributed = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    ActiveEducators = table.Column<int>(type: "int", nullable: false),
                    EducationFundingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyAnalytics", x => x.Id);
                });

            // Create PerformanceMetrics table
            migrationBuilder.CreateTable(
                name: "PerformanceMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MetricName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false, computedColumnSql: "CAST([Timestamp] AS DATE)"),
                    PreviousValue = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    ChangePercentage = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                });

            // Create TierSnapshots table
            migrationBuilder.CreateTable(
                name: "TierSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalyticsSnapshotId = table.Column<int>(type: "int", nullable: false),
                    TierId = table.Column<int>(type: "int", nullable: false),
                    TierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Allocation = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Sold = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    SoldChange24h = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TierSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TierSnapshots_AnalyticsSnapshots_AnalyticsSnapshotId",
                        column: x => x.AnalyticsSnapshotId,
                        principalTable: "AnalyticsSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for AnalyticsSnapshots
            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsSnapshots_Timestamp",
                table: "AnalyticsSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsSnapshots_Date",
                table: "AnalyticsSnapshots",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsSnapshots_Date_Timestamp",
                table: "AnalyticsSnapshots",
                columns: new[] { "Date", "Timestamp" });

            // Create indexes for DailyAnalytics
            migrationBuilder.CreateIndex(
                name: "IX_DailyAnalytics_Date",
                table: "DailyAnalytics",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyAnalytics_Date_DailyVolume",
                table: "DailyAnalytics",
                columns: new[] { "Date", "DailyVolume" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyAnalytics_Date_DailyUsdRaised",
                table: "DailyAnalytics",
                columns: new[] { "Date", "DailyUsdRaised" });

            // Create indexes for PerformanceMetrics
            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_MetricName",
                table: "PerformanceMetrics",
                column: "MetricName");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_Category",
                table: "PerformanceMetrics",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_Timestamp",
                table: "PerformanceMetrics",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_Date",
                table: "PerformanceMetrics",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_IsPublic",
                table: "PerformanceMetrics",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_MetricName_Timestamp",
                table: "PerformanceMetrics",
                columns: new[] { "MetricName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_Category_Timestamp",
                table: "PerformanceMetrics",
                columns: new[] { "Category", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_MetricName_Category_IsPublic",
                table: "PerformanceMetrics",
                columns: new[] { "MetricName", "Category", "IsPublic" });

            // Create indexes for TierSnapshots
            migrationBuilder.CreateIndex(
                name: "IX_TierSnapshots_TierId",
                table: "TierSnapshots",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_TierSnapshots_Timestamp",
                table: "TierSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TierSnapshots_TierId_Timestamp",
                table: "TierSnapshots",
                columns: new[] { "TierId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_TierSnapshots_AnalyticsSnapshotId",
                table: "TierSnapshots",
                column: "AnalyticsSnapshotId");

            // Add indexes to existing tables for analytics queries
            migrationBuilder.CreateIndex(
                name: "IX_PurchaseTransactions_CreatedAt",
                table: "PurchaseTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimTransactions_CreatedAt",
                table: "ClaimTransactions",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop indexes on existing tables
            migrationBuilder.DropIndex(
                name: "IX_PurchaseTransactions_CreatedAt",
                table: "PurchaseTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ClaimTransactions_CreatedAt",
                table: "ClaimTransactions");

            // Drop TierSnapshots table and indexes
            migrationBuilder.DropTable(
                name: "TierSnapshots");

            // Drop PerformanceMetrics table and indexes
            migrationBuilder.DropTable(
                name: "PerformanceMetrics");

            // Drop DailyAnalytics table and indexes
            migrationBuilder.DropTable(
                name: "DailyAnalytics");

            // Drop AnalyticsSnapshots table and indexes
            migrationBuilder.DropTable(
                name: "AnalyticsSnapshots");
        }
    }
}