using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachCrowdSale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCompleteSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    PriceChange24h = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    HoldersCount = table.Column<int>(type: "int", nullable: false),
                    TotalRaised = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TokensSold = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    ParticipantsCount = table.Column<int>(type: "int", nullable: false),
                    ActiveTierId = table.Column<int>(type: "int", nullable: false),
                    TotalValueLocked = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StakedTokens = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    RewardsDistributed = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    ActiveStakers = table.Column<int>(type: "int", nullable: false),
                    TreasuryBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StabilityFundBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BurnedTokens = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    TransactionsCount24h = table.Column<int>(type: "int", nullable: false),
                    UniqueUsers24h = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BurnEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
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
                    HistoricalBurns = table.Column<long>(type: "bigint", precision: 18, scale: 0, nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
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
                    DailyTokensSold = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "DemoProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FundingGoal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CurrentFunding = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeacherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentsImpacted = table.Column<int>(type: "int", nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoProjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DexConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BaseUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApiUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultFeePercentage = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsRecommended = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Network = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChainId = table.Column<int>(type: "int", nullable: false),
                    RouterAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: true),
                    FactoryAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DexConfigurations", x => x.Id);
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
                    VotesFor = table.Column<long>(type: "bigint", precision: 18, scale: 0, nullable: false),
                    VotesAgainst = table.Column<long>(type: "bigint", precision: 18, scale: 0, nullable: false),
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
                    VotingPower = table.Column<long>(type: "bigint", precision: 18, scale: 0, nullable: false),
                    VoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GovernanceVotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LiquidityPools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DexName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TokenPair = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PoolAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    Token0Address = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    Token1Address = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    Token0Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Token1Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Token0Decimals = table.Column<int>(type: "int", nullable: false),
                    Token1Decimals = table.Column<int>(type: "int", nullable: false),
                    TotalValueLocked = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Volume24h = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Volume7d = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FeePercentage = table.Column<decimal>(type: "decimal(5,3)", precision: 5, scale: 3, nullable: false),
                    APY = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    APR = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    Token0Reserve = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Token1Reserve = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    CurrentPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsRecommended = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DexUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AnalyticsUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidityPools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PencilDrives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PencilGoal = table.Column<int>(type: "int", nullable: false),
                    TokensRaised = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    PencilsDistributed = table.Column<int>(type: "int", nullable: false),
                    SchoolsApplied = table.Column<int>(type: "int", nullable: false),
                    SchoolsApproved = table.Column<int>(type: "int", nullable: false),
                    PartnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PartnerLogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PartnerPencilsCommitted = table.Column<int>(type: "int", nullable: false),
                    PlatformPencilsCommitted = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PencilDrives", x => x.Id);
                });

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
                    PreviousValue = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    ChangePercentage = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerformanceMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReferrerUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    TimeOnPage = table.Column<int>(type: "int", nullable: false),
                    ConversionAction = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConversionData = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAnalytics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformWaitlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchoolDistrict = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TeachingSubject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InterestedInTEACHTokens = table.Column<bool>(type: "bit", nullable: false),
                    SubscribeToUpdates = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformWaitlists", x => x.Id);
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
                name: "SchoolBeneficiaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WalletAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Website = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StudentCount = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    VerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalReceived = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolBeneficiaries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StakingPools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    MinStakeAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    MaxStakeAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    LockPeriodDays = table.Column<int>(type: "int", nullable: false),
                    BaseAPY = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    BonusAPY = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    TotalStaked = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    MaxPoolSize = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    EarlyWithdrawalPenalty = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakingPools", x => x.Id);
                });

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
                    TokenAmount = table.Column<long>(type: "bigint", precision: 18, scale: 0, nullable: false),
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
                    TotalSupply = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    CirculatingSupply = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
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
                });

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

            migrationBuilder.CreateTable(
                name: "UserBalances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    TotalPurchased = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    TotalClaimed = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    PendingTokens = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBalances", x => x.Id);
                });

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
                name: "VestingCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TotalTokens = table.Column<long>(type: "bigint", precision: 18, scale: 0, nullable: false),
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
                name: "VestingEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnlockDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokensUnlocked = table.Column<long>(type: "bigint", precision: 18, scale: 0, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "VestingMilestones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokensUnlocked = table.Column<long>(type: "bigint", nullable: false),
                    CumulativeUnlocked = table.Column<long>(type: "bigint", nullable: false),
                    PercentageUnlocked = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsExecuted = table.Column<bool>(type: "bit", nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FormattedDate = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(32,18)", precision: 32, scale: 18, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VestingMilestones", x => x.Id);
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
                    Allocation = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    Sold = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
                    SoldChange24h = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "LiquidityPoolSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LiquidityPoolId = table.Column<int>(type: "int", nullable: false),
                    TotalValueLocked = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Volume24h = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Token0Reserve = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Token1Reserve = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    APY = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    APR = table.Column<decimal>(type: "decimal(8,4)", precision: 8, scale: 4, nullable: false),
                    ActivePositions = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsLatest = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidityPoolSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidityPoolSnapshots_LiquidityPools_LiquidityPoolId",
                        column: x => x.LiquidityPoolId,
                        principalTable: "LiquidityPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLiquidityPositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    LiquidityPoolId = table.Column<int>(type: "int", nullable: false),
                    LpTokenAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Token0Amount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Token1Amount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    InitialValueUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrentValueUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FeesEarnedUsd = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    ImpermanentLoss = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    NetPnL = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AddTransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: true),
                    RemoveTransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLiquidityPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserLiquidityPositions_LiquidityPools_LiquidityPoolId",
                        column: x => x.LiquidityPoolId,
                        principalTable: "LiquidityPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PencilDriveImpactStories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PencilDriveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeacherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PencilsReceived = table.Column<int>(type: "int", nullable: false),
                    StudentsImpacted = table.Column<int>(type: "int", nullable: false),
                    StoryText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PencilDriveImpactStories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PencilDriveImpactStories_PencilDrives_PencilDriveId",
                        column: x => x.PencilDriveId,
                        principalTable: "PencilDrives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolRewardDistributions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolBeneficiaryId = table.Column<int>(type: "int", nullable: false),
                    StakerAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    DistributionDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolRewardDistributions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolRewardDistributions_SchoolBeneficiaries_SchoolBeneficiaryId",
                        column: x => x.SchoolBeneficiaryId,
                        principalTable: "SchoolBeneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserStakingBeneficiaries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    SchoolBeneficiaryId = table.Column<int>(type: "int", nullable: false),
                    SelectedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    TotalDonated = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStakingBeneficiaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStakingBeneficiaries_SchoolBeneficiaries_SchoolBeneficiaryId",
                        column: x => x.SchoolBeneficiaryId,
                        principalTable: "SchoolBeneficiaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserStakes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    StakingPoolId = table.Column<int>(type: "int", nullable: false),
                    StakedAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    AccruedRewards = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    ClaimedRewards = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    StakeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnstakeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRewardCalculation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastClaimDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StakeTransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: true),
                    UnstakeTransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStakes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserStakes_StakingPools_StakingPoolId",
                        column: x => x.StakingPoolId,
                        principalTable: "StakingPools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClaimTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    TokenAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserBalanceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClaimTransactions_UserBalances_UserBalanceId",
                        column: x => x.UserBalanceId,
                        principalTable: "UserBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    TierId = table.Column<int>(type: "int", nullable: false),
                    UsdAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TokenAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    TokenPrice = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserBalanceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseTransactions_UserBalances_UserBalanceId",
                        column: x => x.UserBalanceId,
                        principalTable: "UserBalances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LiquidityTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserLiquidityPositionId = table.Column<int>(type: "int", nullable: false),
                    WalletAddress = table.Column<string>(type: "nvarchar(42)", maxLength: 42, nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Token0Amount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    Token1Amount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    LpTokenAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    ValueUsd = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GasFeesUsd = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiquidityTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LiquidityTransactions_UserLiquidityPositions_UserLiquidityPositionId",
                        column: x => x.UserLiquidityPositionId,
                        principalTable: "UserLiquidityPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StakingRewardClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserStakeId = table.Column<int>(type: "int", nullable: false),
                    ClaimedAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    ClaimDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionHash = table.Column<string>(type: "nvarchar(66)", maxLength: 66, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakingRewardClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StakingRewardClaims_UserStakes_UserStakeId",
                        column: x => x.UserStakeId,
                        principalTable: "UserStakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsSnapshots_ActiveTierId",
                table: "AnalyticsSnapshots",
                column: "ActiveTierId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsSnapshots_Timestamp",
                table: "AnalyticsSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_BurnEvents_Date",
                table: "BurnEvents",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_BurnEvents_IsVerified",
                table: "BurnEvents",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_BurnEvents_Mechanism",
                table: "BurnEvents",
                column: "Mechanism");

            migrationBuilder.CreateIndex(
                name: "IX_BurnEvents_TransactionHash",
                table: "BurnEvents",
                column: "TransactionHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BurnMechanisms_IsActive",
                table: "BurnMechanisms",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_BurnMechanisms_Name",
                table: "BurnMechanisms",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BurnSnapshots_IsLatest",
                table: "BurnSnapshots",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_BurnSnapshots_LastBurnDate",
                table: "BurnSnapshots",
                column: "LastBurnDate");

            migrationBuilder.CreateIndex(
                name: "IX_BurnSnapshots_Timestamp",
                table: "BurnSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimTransactions_CreatedAt",
                table: "ClaimTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimTransactions_TransactionHash",
                table: "ClaimTransactions",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimTransactions_UserBalanceId",
                table: "ClaimTransactions",
                column: "UserBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimTransactions_WalletAddress",
                table: "ClaimTransactions",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_DailyAnalytics_Date",
                table: "DailyAnalytics",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_Category",
                table: "DemoProjects",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_CreatedAt",
                table: "DemoProjects",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_Deadline",
                table: "DemoProjects",
                column: "Deadline");

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_IsActive",
                table: "DemoProjects",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_IsFeatured",
                table: "DemoProjects",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_IsUrgent",
                table: "DemoProjects",
                column: "IsUrgent");

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_State",
                table: "DemoProjects",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_DexConfigurations_IsActive",
                table: "DexConfigurations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DexConfigurations_IsRecommended",
                table: "DexConfigurations",
                column: "IsRecommended");

            migrationBuilder.CreateIndex(
                name: "IX_DexConfigurations_Name",
                table: "DexConfigurations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DexConfigurations_Network",
                table: "DexConfigurations",
                column: "Network");

            migrationBuilder.CreateIndex(
                name: "IX_DexConfigurations_SortOrder",
                table: "DexConfigurations",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceProposals_Category",
                table: "GovernanceProposals",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceProposals_EndDate",
                table: "GovernanceProposals",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceProposals_ProposerAddress",
                table: "GovernanceProposals",
                column: "ProposerAddress");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceProposals_StartDate",
                table: "GovernanceProposals",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceProposals_Status",
                table: "GovernanceProposals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceVotes_ProposalId",
                table: "GovernanceVotes",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceVotes_ProposalId_VoterAddress",
                table: "GovernanceVotes",
                columns: new[] { "ProposalId", "VoterAddress" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceVotes_VoteDate",
                table: "GovernanceVotes",
                column: "VoteDate");

            migrationBuilder.CreateIndex(
                name: "IX_GovernanceVotes_VoterAddress",
                table: "GovernanceVotes",
                column: "VoterAddress");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPools_DexName",
                table: "LiquidityPools",
                column: "DexName");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPools_IsActive",
                table: "LiquidityPools",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPools_IsFeatured",
                table: "LiquidityPools",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPools_LastSyncAt",
                table: "LiquidityPools",
                column: "LastSyncAt");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPools_PoolAddress",
                table: "LiquidityPools",
                column: "PoolAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPools_TokenPair",
                table: "LiquidityPools",
                column: "TokenPair");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPools_TotalValueLocked",
                table: "LiquidityPools",
                column: "TotalValueLocked");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPoolSnapshots_LiquidityPoolId",
                table: "LiquidityPoolSnapshots",
                column: "LiquidityPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPoolSnapshots_LiquidityPoolId_IsLatest",
                table: "LiquidityPoolSnapshots",
                columns: new[] { "LiquidityPoolId", "IsLatest" });

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPoolSnapshots_LiquidityPoolId_Timestamp",
                table: "LiquidityPoolSnapshots",
                columns: new[] { "LiquidityPoolId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPoolSnapshots_Source",
                table: "LiquidityPoolSnapshots",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityPoolSnapshots_Timestamp",
                table: "LiquidityPoolSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityTransactions_Status",
                table: "LiquidityTransactions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityTransactions_Timestamp",
                table: "LiquidityTransactions",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityTransactions_TransactionHash",
                table: "LiquidityTransactions",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityTransactions_TransactionType",
                table: "LiquidityTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityTransactions_UserLiquidityPositionId",
                table: "LiquidityTransactions",
                column: "UserLiquidityPositionId");

            migrationBuilder.CreateIndex(
                name: "IX_LiquidityTransactions_WalletAddress",
                table: "LiquidityTransactions",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_CreatedAt",
                table: "PencilDriveImpactStories",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_IsActive",
                table: "PencilDriveImpactStories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_IsFeatured",
                table: "PencilDriveImpactStories",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_PencilDriveId",
                table: "PencilDriveImpactStories",
                column: "PencilDriveId");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_State",
                table: "PencilDriveImpactStories",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDrives_EndDate",
                table: "PencilDrives",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDrives_IsActive",
                table: "PencilDrives",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDrives_StartDate",
                table: "PencilDrives",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDrives_Year",
                table: "PencilDrives",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_Category",
                table: "PerformanceMetrics",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_Category_Timestamp",
                table: "PerformanceMetrics",
                columns: new[] { "Category", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_IsPublic",
                table: "PerformanceMetrics",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_MetricName",
                table: "PerformanceMetrics",
                column: "MetricName");

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_MetricName_Timestamp",
                table: "PerformanceMetrics",
                columns: new[] { "MetricName", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PerformanceMetrics_Timestamp",
                table: "PerformanceMetrics",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAnalytics_ConversionAction",
                table: "PlatformAnalytics",
                column: "ConversionAction");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAnalytics_CreatedAt",
                table: "PlatformAnalytics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAnalytics_PageUrl",
                table: "PlatformAnalytics",
                column: "PageUrl");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAnalytics_SessionId",
                table: "PlatformAnalytics",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWaitlists_CreatedAt",
                table: "PlatformWaitlists",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWaitlists_Email",
                table: "PlatformWaitlists",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWaitlists_InterestedInTEACHTokens",
                table: "PlatformWaitlists",
                column: "InterestedInTEACHTokens");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWaitlists_UserType",
                table: "PlatformWaitlists",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_Pair_Timestamp",
                table: "PriceHistory",
                columns: new[] { "Pair", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_Source_Timestamp",
                table: "PriceHistory",
                columns: new[] { "Source", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistory_Timestamp",
                table: "PriceHistory",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseTransactions_CreatedAt",
                table: "PurchaseTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseTransactions_TierId",
                table: "PurchaseTransactions",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseTransactions_TransactionHash",
                table: "PurchaseTransactions",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseTransactions_UserBalanceId",
                table: "PurchaseTransactions",
                column: "UserBalanceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseTransactions_WalletAddress",
                table: "PurchaseTransactions",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_Country",
                table: "SchoolBeneficiaries",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_IsActive",
                table: "SchoolBeneficiaries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_IsVerified",
                table: "SchoolBeneficiaries",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_State",
                table: "SchoolBeneficiaries",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_WalletAddress",
                table: "SchoolBeneficiaries",
                column: "WalletAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRewardDistributions_DistributionDate",
                table: "SchoolRewardDistributions",
                column: "DistributionDate");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRewardDistributions_SchoolBeneficiaryId",
                table: "SchoolRewardDistributions",
                column: "SchoolBeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRewardDistributions_StakerAddress",
                table: "SchoolRewardDistributions",
                column: "StakerAddress");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRewardDistributions_Status",
                table: "SchoolRewardDistributions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRewardDistributions_TransactionHash",
                table: "SchoolRewardDistributions",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_StakingPools_IsActive",
                table: "StakingPools",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_StakingPools_LockPeriodDays",
                table: "StakingPools",
                column: "LockPeriodDays");

            migrationBuilder.CreateIndex(
                name: "IX_StakingRewardClaims_ClaimDate",
                table: "StakingRewardClaims",
                column: "ClaimDate");

            migrationBuilder.CreateIndex(
                name: "IX_StakingRewardClaims_TransactionHash",
                table: "StakingRewardClaims",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_StakingRewardClaims_UserStakeId",
                table: "StakingRewardClaims",
                column: "UserStakeId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplySnapshots_IsLatest",
                table: "SupplySnapshots",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_SupplySnapshots_Timestamp",
                table: "SupplySnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TierSnapshots_AnalyticsSnapshotId",
                table: "TierSnapshots",
                column: "AnalyticsSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_TierSnapshots_IsActive",
                table: "TierSnapshots",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TierSnapshots_TierId",
                table: "TierSnapshots",
                column: "TierId");

            migrationBuilder.CreateIndex(
                name: "IX_TierSnapshots_TierId_Timestamp",
                table: "TierSnapshots",
                columns: new[] { "TierId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_TierSnapshots_Timestamp",
                table: "TierSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TokenAllocations_Category",
                table: "TokenAllocations",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TokenAllocations_UnlockDate",
                table: "TokenAllocations",
                column: "UnlockDate");

            migrationBuilder.CreateIndex(
                name: "IX_TokenMetricsSnapshots_Source",
                table: "TokenMetricsSnapshots",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_TokenMetricsSnapshots_Timestamp",
                table: "TokenMetricsSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TokenMetricsSnapshots_Timestamp_IsLatest",
                table: "TokenMetricsSnapshots",
                columns: new[] { "Timestamp", "IsLatest" });

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryAllocations_Category",
                table: "TreasuryAllocations",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryAllocations_SnapshotId",
                table: "TreasuryAllocations",
                column: "SnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_TreasurySnapshots_IsLatest",
                table: "TreasurySnapshots",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_TreasurySnapshots_Source",
                table: "TreasurySnapshots",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_TreasurySnapshots_Timestamp",
                table: "TreasurySnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_Category",
                table: "TreasuryTransactions",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_IsVerified",
                table: "TreasuryTransactions",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_Timestamp",
                table: "TreasuryTransactions",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_TransactionHash",
                table: "TreasuryTransactions",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_TreasuryTransactions_TransactionType",
                table: "TreasuryTransactions",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalances_LastUpdated",
                table: "UserBalances",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_UserBalances_WalletAddress",
                table: "UserBalances",
                column: "WalletAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLiquidityPositions_AddedAt",
                table: "UserLiquidityPositions",
                column: "AddedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserLiquidityPositions_IsActive",
                table: "UserLiquidityPositions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserLiquidityPositions_LiquidityPoolId",
                table: "UserLiquidityPositions",
                column: "LiquidityPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLiquidityPositions_WalletAddress",
                table: "UserLiquidityPositions",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserLiquidityPositions_WalletAddress_IsActive",
                table: "UserLiquidityPositions",
                columns: new[] { "WalletAddress", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UserStakes_IsActive",
                table: "UserStakes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakes_StakeDate",
                table: "UserStakes",
                column: "StakeDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakes_StakingPoolId",
                table: "UserStakes",
                column: "StakingPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakes_WalletAddress",
                table: "UserStakes",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakingBeneficiaries_IsActive",
                table: "UserStakingBeneficiaries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakingBeneficiaries_SchoolBeneficiaryId",
                table: "UserStakingBeneficiaries",
                column: "SchoolBeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakingBeneficiaries_WalletAddress",
                table: "UserStakingBeneficiaries",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakingBeneficiaries_WalletAddress_IsActive",
                table: "UserStakingBeneficiaries",
                columns: new[] { "WalletAddress", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityMetricsSnapshots_IsLatest",
                table: "UtilityMetricsSnapshots",
                column: "IsLatest");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityMetricsSnapshots_Source",
                table: "UtilityMetricsSnapshots",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_UtilityMetricsSnapshots_Timestamp",
                table: "UtilityMetricsSnapshots",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_VestingCategories_Category",
                table: "VestingCategories",
                column: "Category",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VestingCategories_IsActive",
                table: "VestingCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_VestingCategories_StartDate",
                table: "VestingCategories",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_VestingEvents_Category",
                table: "VestingEvents",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_VestingEvents_IsProcessed",
                table: "VestingEvents",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_VestingEvents_TransactionHash",
                table: "VestingEvents",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_VestingEvents_UnlockDate",
                table: "VestingEvents",
                column: "UnlockDate");

            migrationBuilder.CreateIndex(
                name: "IX_VestingMilestones_CategoryId_Date",
                table: "VestingMilestones",
                columns: new[] { "CategoryId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_VestingMilestones_Date",
                table: "VestingMilestones",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_VestingMilestones_IsExecuted",
                table: "VestingMilestones",
                column: "IsExecuted");

            migrationBuilder.CreateIndex(
                name: "IX_VolumeHistory_Source_Timestamp",
                table: "VolumeHistory",
                columns: new[] { "Source", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_VolumeHistory_Timeframe_Timestamp",
                table: "VolumeHistory",
                columns: new[] { "Timeframe", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_VolumeHistory_Timestamp",
                table: "VolumeHistory",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BurnEvents");

            migrationBuilder.DropTable(
                name: "BurnMechanisms");

            migrationBuilder.DropTable(
                name: "BurnSnapshots");

            migrationBuilder.DropTable(
                name: "ClaimTransactions");

            migrationBuilder.DropTable(
                name: "DailyAnalytics");

            migrationBuilder.DropTable(
                name: "DemoProjects");

            migrationBuilder.DropTable(
                name: "DexConfigurations");

            migrationBuilder.DropTable(
                name: "GovernanceProposals");

            migrationBuilder.DropTable(
                name: "GovernanceVotes");

            migrationBuilder.DropTable(
                name: "LiquidityPoolSnapshots");

            migrationBuilder.DropTable(
                name: "LiquidityTransactions");

            migrationBuilder.DropTable(
                name: "PencilDriveImpactStories");

            migrationBuilder.DropTable(
                name: "PerformanceMetrics");

            migrationBuilder.DropTable(
                name: "PlatformAnalytics");

            migrationBuilder.DropTable(
                name: "PlatformWaitlists");

            migrationBuilder.DropTable(
                name: "PriceHistory");

            migrationBuilder.DropTable(
                name: "PurchaseTransactions");

            migrationBuilder.DropTable(
                name: "SchoolRewardDistributions");

            migrationBuilder.DropTable(
                name: "StakingRewardClaims");

            migrationBuilder.DropTable(
                name: "SupplySnapshots");

            migrationBuilder.DropTable(
                name: "TierSnapshots");

            migrationBuilder.DropTable(
                name: "TokenAllocations");

            migrationBuilder.DropTable(
                name: "TokenMetricsSnapshots");

            migrationBuilder.DropTable(
                name: "TreasuryAllocations");

            migrationBuilder.DropTable(
                name: "TreasurySnapshots");

            migrationBuilder.DropTable(
                name: "TreasuryTransactions");

            migrationBuilder.DropTable(
                name: "UserStakingBeneficiaries");

            migrationBuilder.DropTable(
                name: "UtilityMetricsSnapshots");

            migrationBuilder.DropTable(
                name: "VestingCategories");

            migrationBuilder.DropTable(
                name: "VestingEvents");

            migrationBuilder.DropTable(
                name: "VestingMilestones");

            migrationBuilder.DropTable(
                name: "VolumeHistory");

            migrationBuilder.DropTable(
                name: "UserLiquidityPositions");

            migrationBuilder.DropTable(
                name: "PencilDrives");

            migrationBuilder.DropTable(
                name: "UserBalances");

            migrationBuilder.DropTable(
                name: "UserStakes");

            migrationBuilder.DropTable(
                name: "AnalyticsSnapshots");

            migrationBuilder.DropTable(
                name: "SchoolBeneficiaries");

            migrationBuilder.DropTable(
                name: "LiquidityPools");

            migrationBuilder.DropTable(
                name: "StakingPools");
        }
    }
}
