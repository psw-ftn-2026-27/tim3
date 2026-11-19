using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizParticipations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizParticipations",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QuizId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentPass = table.Column<int>(type: "integer", nullable: false),
                    SolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExperienceGained = table.Column<int>(type: "integer", nullable: true),
                    PassQuestionIds = table.Column<List<Guid>>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'::uuid[]"),
                    Answers = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizParticipations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizParticipations_QuizId_ExplorerId",
                schema: "games",
                table: "QuizParticipations",
                columns: new[] { "QuizId", "ExplorerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizParticipations",
                schema: "games");
        }
    }
}
