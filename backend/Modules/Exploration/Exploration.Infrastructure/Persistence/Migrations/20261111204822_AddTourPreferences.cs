using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTourPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TourPreferences",
                schema: "exploration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Difficulties = table.Column<string[]>(type: "text[]", nullable: false),
                    Transport = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    AppliesToSearch = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourPreferences", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TourPreferences_ExplorerId",
                schema: "exploration",
                table: "TourPreferences",
                column: "ExplorerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TourPreferences",
                schema: "exploration");
        }
    }
}
