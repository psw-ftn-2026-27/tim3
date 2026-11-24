using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationChallengeParticipations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationChallengeParticipations",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SearchCircle_Center_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    SearchCircle_Center_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    SearchCircle_Radius = table.Column<double>(type: "double precision", nullable: false),
                    DiscoveredPointIds = table.Column<Guid[]>(type: "uuid[]", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExperienceGained = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationChallengeParticipations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationChallengeParticipations_ExplorerId_LocationChalleng~",
                schema: "games",
                table: "LocationChallengeParticipations",
                columns: new[] { "ExplorerId", "LocationChallengeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationChallengeParticipations",
                schema: "games");
        }
    }
}
