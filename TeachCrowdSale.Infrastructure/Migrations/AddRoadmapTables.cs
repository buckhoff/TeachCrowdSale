// TeachCrowdSale.Infrastructure/Migrations/[TIMESTAMP]_AddRoadmapTables.cs
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachCrowdSale.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoadmapTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Milestones table
            migrationBuilder.CreateTable(
                name: "Milestones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProgressPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    TechnicalDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GitHubIssueUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DocumentationUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Milestones", x => x.Id);
                });

            // Create DevelopmentTasks table
            migrationBuilder.CreateTable(
                name: "DevelopmentTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MilestoneId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EstimatedCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProgressPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AssignedDeveloper = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GitHubIssueUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PullRequestUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevelopmentTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevelopmentTasks_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create Dependencies table
            migrationBuilder.CreateTable(
                name: "Dependencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MilestoneId = table.Column<int>(type: "int", nullable: false),
                    DependsOnMilestoneId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dependencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dependencies_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dependencies_Milestones_DependsOnMilestoneId",
                        column: x => x.DependsOnMilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create Updates table
            migrationBuilder.CreateTable(
                name: "Updates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MilestoneId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ProgressChange = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AuthorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Updates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Updates_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create Releases table
            migrationBuilder.CreateTable(
                name: "Releases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PlannedReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleaseNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GitHubReleaseUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DocumentationUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Releases", x => x.Id);
                });

            // Create ReleaseMilestones junction table
            migrationBuilder.CreateTable(
                name: "ReleaseMilestones",
                columns: table => new
                {
                    MilestoneId = table.Column<int>(type: "int", nullable: false),
                    ReleaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReleaseMilestones", x => new { x.MilestoneId, x.ReleaseId });
                    table.ForeignKey(
                        name: "FK_ReleaseMilestones_Milestones_MilestoneId",
                        column: x => x.MilestoneId,
                        principalTable: "Milestones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReleaseMilestones_Releases_ReleaseId",
                        column: x => x.ReleaseId,
                        principalTable: "Releases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_Milestones_Status",
                table: "Milestones",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_Category",
                table: "Milestones",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_SortOrder",
                table: "Milestones",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Milestones_EstimatedCompletionDate",
                table: "Milestones",
                column: "EstimatedCompletionDate");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentTasks_MilestoneId",
                table: "DevelopmentTasks",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentTasks_Status",
                table: "DevelopmentTasks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentTasks_Type",
                table: "DevelopmentTasks",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentTasks_AssignedDeveloper",
                table: "DevelopmentTasks",
                column: "AssignedDeveloper");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentTasks_SortOrder",
                table: "DevelopmentTasks",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_Dependencies_MilestoneId",
                table: "Dependencies",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Dependencies_DependsOnMilestoneId",
                table: "Dependencies",
                column: "DependsOnMilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Dependencies_Type",
                table: "Dependencies",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Updates_MilestoneId",
                table: "Updates",
                column: "MilestoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Updates_Type",
                table: "Updates",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Updates_CreatedAt",
                table: "Updates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Updates_IsPublic",
                table: "Updates",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_Version",
                table: "Releases",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Releases_Type",
                table: "Releases",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_Status",
                table: "Releases",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_PlannedReleaseDate",
                table: "Releases",
                column: "PlannedReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_ActualReleaseDate",
                table: "Releases",
                column: "ActualReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Releases_IsPublic",
                table: "Releases",
                column: "IsPublic");

            migrationBuilder.CreateIndex(
                name: "IX_ReleaseMilestones_ReleaseId",
                table: "ReleaseMilestones",
                column: "ReleaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ReleaseMilestones");
            migrationBuilder.DropTable(name: "Updates");
            migrationBuilder.DropTable(name: "Dependencies");
            migrationBuilder.DropTable(name: "DevelopmentTasks");
            migrationBuilder.DropTable(name: "Releases");
            migrationBuilder.DropTable(name: "Milestones");
        }
    }
}