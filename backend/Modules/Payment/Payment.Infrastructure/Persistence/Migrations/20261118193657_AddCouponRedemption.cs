using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCouponRedemption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscountInEc",
                schema: "payment",
                table: "WalletTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CouponId",
                schema: "payment",
                table: "TourPurchaseTokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountInEc",
                schema: "payment",
                table: "TourPurchaseTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "CouponIds",
                schema: "payment",
                table: "ShoppingCarts",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'::uuid[]");

            migrationBuilder.AddColumn<Guid>(
                name: "AuthorId",
                schema: "payment",
                table: "OrderItems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "Sequence",
                schema: "payment",
                table: "OrderItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountInEc",
                schema: "payment",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "CouponId",
                schema: "payment",
                table: "TourPurchaseTokens");

            migrationBuilder.DropColumn(
                name: "DiscountInEc",
                schema: "payment",
                table: "TourPurchaseTokens");

            migrationBuilder.DropColumn(
                name: "CouponIds",
                schema: "payment",
                table: "ShoppingCarts");

            migrationBuilder.DropColumn(
                name: "AuthorId",
                schema: "payment",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Sequence",
                schema: "payment",
                table: "OrderItems");
        }
    }
}
