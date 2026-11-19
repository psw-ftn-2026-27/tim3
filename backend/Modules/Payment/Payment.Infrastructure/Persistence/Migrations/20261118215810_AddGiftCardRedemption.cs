using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftCardRedemption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GiftCardInEc",
                schema: "payment",
                table: "WalletTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "GiftCardId",
                schema: "payment",
                table: "ShoppingCarts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RedeemedAt",
                schema: "payment",
                table: "GiftCards",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GiftCardInEc",
                schema: "payment",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "GiftCardId",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "RedeemedAt",
                schema: "payment",
                table: "GiftCards");
        }
    }
}
