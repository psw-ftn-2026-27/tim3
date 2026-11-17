using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chats.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMutedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "MutedUserIds",
                schema: "chats",
                table: "Chats",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'::uuid[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MutedUserIds",
                schema: "chats",
                table: "Chats");
        }
    }
}
