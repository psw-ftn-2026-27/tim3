using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "maps");

            migrationBuilder.CreateTable(
                name: "UserPositions",
                schema: "maps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false),
                    SetAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPositions_UserId",
                schema: "maps",
                table: "UserPositions",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPositions",
                schema: "maps");
        }
    }
}
