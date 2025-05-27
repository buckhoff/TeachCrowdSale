using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachCrowdSale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "IX_UserBalances_WalletAddress",
                table: "UserBalances",
                column: "WalletAddress",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClaimTransactions");

            migrationBuilder.DropTable(
                name: "PurchaseTransactions");

            migrationBuilder.DropTable(
                name: "UserBalances");
        }
    }
}
