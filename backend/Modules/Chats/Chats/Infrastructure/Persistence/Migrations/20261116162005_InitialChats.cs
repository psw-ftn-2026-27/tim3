using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chats.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialChats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "chats");

            migrationBuilder.CreateTable(
                name: "Chats",
                schema: "chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject_Kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Subject_FirstId = table.Column<Guid>(type: "uuid", nullable: false),
                    Subject_SecondId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "chats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChatId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Chats_ChatId",
                        column: x => x.ChatId,
                        principalSchema: "chats",
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chats_Subject_Kind_Subject_FirstId_Subject_SecondId",
                schema: "chats",
                table: "Chats",
                columns: new[] { "Subject_Kind", "Subject_FirstId", "Subject_SecondId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChatId",
                schema: "chats",
                table: "Messages",
                column: "ChatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages",
                schema: "chats");

            migrationBuilder.DropTable(
                name: "Chats",
                schema: "chats");
        }
    }
}
