// Migration to add PencilImpact entities
// Run: Add-Migration AddPencilImpactEntities
// Then: Update-Database

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachCrowdSale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPencilImpactEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PlatformWaitlist table
            migrationBuilder.CreateTable(
                name: "PlatformWaitlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    UserType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SchoolDistrict = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TeachingSubject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InterestedInTEACHTokens = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    SubscribeToUpdates = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformWaitlists", x => x.Id);
                });

            // PencilDrives table
            migrationBuilder.CreateTable(
                name: "PencilDrives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PencilGoal = table.Column<int>(type: "int", nullable: false, defaultValue: 2000000),
                    TokensRaised = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false, defaultValue: 0m),
                    PencilsDistributed = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SchoolsApplied = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SchoolsApproved = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    PartnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PartnerLogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PartnerPencilsCommitted = table.Column<int>(type: "int", nullable: false, defaultValue: 500000),
                    PlatformPencilsCommitted = table.Column<int>(type: "int", nullable: false, defaultValue: 500000),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PencilDrives", x => x.Id);
                });

            // DemoProjects table
            migrationBuilder.CreateTable(
                name: "DemoProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FundingGoal = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CurrentFunding = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m),
                    SchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeacherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StudentsImpacted = table.Column<int>(type: "int", nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemoProjects", x => x.Id);
                });

            // PlatformAnalytics table
            migrationBuilder.CreateTable(
                name: "PlatformAnalytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReferrerUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UserIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    TimeOnPage = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ConversionAction = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConversionData = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformAnalytics", x => x.Id);
                });

            // PencilDriveImpactStories table
            migrationBuilder.CreateTable(
                name: "PencilDriveImpactStories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWID()"),
                    PencilDriveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchoolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeacherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PencilsReceived = table.Column<int>(type: "int", nullable: false),
                    StudentsImpacted = table.Column<int>(type: "int", nullable: false),
                    StoryText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
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

            // Create indexes
            migrationBuilder.CreateIndex(
                name: "IX_PlatformWaitlists_Email",
                table: "PlatformWaitlists",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWaitlists_UserType",
                table: "PlatformWaitlists",
                column: "UserType");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWaitlists_CreatedAt",
                table: "PlatformWaitlists",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformWaitlists_InterestedInTEACHTokens",
                table: "PlatformWaitlists",
                column: "InterestedInTEACHTokens");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDrives_Year",
                table: "PencilDrives",
                column: "Year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PencilDrives_IsActive",
                table: "PencilDrives",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDrives_StartDate",
                table: "PencilDrives",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDrives_EndDate",
                table: "PencilDrives",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_Category",
                table: "DemoProjects",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_State",
                table: "DemoProjects",
                column: "State");

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
                name: "IX_DemoProjects_Deadline",
                table: "DemoProjects",
                column: "Deadline");

            migrationBuilder.CreateIndex(
                name: "IX_DemoProjects_CreatedAt",
                table: "DemoProjects",
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
                name: "IX_PlatformAnalytics_ConversionAction",
                table: "PlatformAnalytics",
                column: "ConversionAction");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformAnalytics_CreatedAt",
                table: "PlatformAnalytics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_PencilDriveId",
                table: "PencilDriveImpactStories",
                column: "PencilDriveId");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_State",
                table: "PencilDriveImpactStories",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_IsFeatured",
                table: "PencilDriveImpactStories",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_IsActive",
                table: "PencilDriveImpactStories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PencilDriveImpactStories_CreatedAt",
                table: "PencilDriveImpactStories",
                column: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PencilDriveImpactStories");

            migrationBuilder.DropTable(
                name: "PlatformAnalytics");

            migrationBuilder.DropTable(
                name: "DemoProjects");

            migrationBuilder.DropTable(
                name: "PlatformWaitlists");

            migrationBuilder.DropTable(
                name: "PencilDrives");
        }
    }
}