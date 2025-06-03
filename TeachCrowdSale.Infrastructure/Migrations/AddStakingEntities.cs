using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachCrowdSale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStakingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // StakingPools table
            migrationBuilder.CreateTable(
                name: "StakingPools",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MinStakeAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    MaxStakeAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    LockPeriodDays = table.Column<int>(type: "int", nullable: false),
                    BaseAPY = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    BonusAPY = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TotalStaked = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    MaxPoolSize = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StakingPools", x => x.Id);
                });

            // SchoolBeneficiaries table
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

            // UserStakes table
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

            // UserStakingBeneficiaries table
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

            // StakingRewardClaims table
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

            // SchoolRewardDistributions table
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

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_StakingPools_IsActive",
                table: "StakingPools",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_StakingPools_LockPeriodDays",
                table: "StakingPools",
                column: "LockPeriodDays");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_WalletAddress",
                table: "SchoolBeneficiaries",
                column: "WalletAddress",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_Country",
                table: "SchoolBeneficiaries",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_State",
                table: "SchoolBeneficiaries",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_IsVerified",
                table: "SchoolBeneficiaries",
                column: "IsVerified");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolBeneficiaries_IsActive",
                table: "SchoolBeneficiaries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakes_WalletAddress",
                table: "UserStakes",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakes_StakingPoolId",
                table: "UserStakes",
                column: "StakingPoolId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakes_IsActive",
                table: "UserStakes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakes_StakeDate",
                table: "UserStakes",
                column: "StakeDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakingBeneficiaries_WalletAddress",
                table: "UserStakingBeneficiaries",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakingBeneficiaries_SchoolBeneficiaryId",
                table: "UserStakingBeneficiaries",
                column: "SchoolBeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakingBeneficiaries_IsActive",
                table: "UserStakingBeneficiaries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserStakingBeneficiaries_WalletAddress_IsActive",
                table: "UserStakingBeneficiaries",
                columns: new[] { "WalletAddress", "IsActive" },
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_StakingRewardClaims_UserStakeId",
                table: "StakingRewardClaims",
                column: "UserStakeId");

            migrationBuilder.CreateIndex(
                name: "IX_StakingRewardClaims_ClaimDate",
                table: "StakingRewardClaims",
                column: "ClaimDate");

            migrationBuilder.CreateIndex(
                name: "IX_StakingRewardClaims_TransactionHash",
                table: "StakingRewardClaims",
                column: "TransactionHash");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRewardDistributions_SchoolBeneficiaryId",
                table: "SchoolRewardDistributions",
                column: "SchoolBeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRewardDistributions_StakerAddress",
                table: "SchoolRewardDistributions",
                column: "StakerAddress");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolRewardDistributions_TransactionHash",
                table: "SchoolRewardDistributions",
                column: "TransactionHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StakingRewardClaims");

            migrationBuilder.DropTable(
                name: "SchoolRewardDistributions");

            migrationBuilder.DropTable(
                name: "UserStakingBeneficiaries");

            migrationBuilder.DropTable(
                name: "UserStakes");

            migrationBuilder.DropTable(
                name: "StakingPools");

            migrationBuilder.DropTable(
                name: "SchoolBeneficiaries");
        }
    }
}