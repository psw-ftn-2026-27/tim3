using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "games");

            migrationBuilder.CreateTable(
                name: "SocialChallenges",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ActivePeriod_Start = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ActivePeriod_End = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    PeopleRequired = table.Column<int>(type: "integer", nullable: false),
                    GatheringRadius = table.Column<double>(type: "double precision", nullable: false),
                    ExperienceGained = table.Column<int>(type: "integer", nullable: false),
                    EndMessage = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialChallenges", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialChallenges",
                schema: "games");
        }
    }
}
