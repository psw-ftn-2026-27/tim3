using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialChallengeExecutions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SocialChallengeExecutions",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialChallengeExecutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SocialChallengeParticipants",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastReportAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastReportInRange = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    ExperienceGained = table.Column<int>(type: "integer", nullable: true),
                    SocialChallengeExecutionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialChallengeParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialChallengeParticipants_SocialChallengeExecutions_Socia~",
                        column: x => x.SocialChallengeExecutionId,
                        principalSchema: "games",
                        principalTable: "SocialChallengeExecutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SocialChallengeExecutions_ChallengeId_Status",
                schema: "games",
                table: "SocialChallengeExecutions",
                columns: new[] { "ChallengeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialChallengeParticipants_ExplorerId",
                schema: "games",
                table: "SocialChallengeParticipants",
                column: "ExplorerId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialChallengeParticipants_SocialChallengeExecutionId",
                schema: "games",
                table: "SocialChallengeParticipants",
                column: "SocialChallengeExecutionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialChallengeParticipants",
                schema: "games");

            migrationBuilder.DropTable(
                name: "SocialChallengeExecutions",
                schema: "games");
        }
    }
}
