using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DodatePoklonKartice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AdministratorId",
                schema: "payment",
                table: "WalletTransactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "GiftCards",
                schema: "payment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCards", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GiftCards_BuyerId",
                schema: "payment",
                table: "GiftCards",
                column: "BuyerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The previous version knows neither gift cards nor their wallet transactions, so the
            // gift card payments are removed and refunded, keeping each balance equal to the sum
            // of its transactions and AdministratorId non-null again.
            migrationBuilder.Sql(@"
                UPDATE payment.""Wallets"" AS wallet
                SET ""Balance"" = wallet.""Balance"" - payments.""Total""
                FROM (
                    SELECT ""WalletId"", SUM(""Amount"") AS ""Total""
                    FROM payment.""WalletTransactions""
                    WHERE ""Kind"" = 'GiftCardPurchase'
                    GROUP BY ""WalletId""
                ) AS payments
                WHERE wallet.""Id"" = payments.""WalletId"";

                DELETE FROM payment.""WalletTransactions"" WHERE ""Kind"" = 'GiftCardPurchase';");

            migrationBuilder.DropTable(
                name: "GiftCards",
                schema: "payment");

            migrationBuilder.AlterColumn<Guid>(
                name: "AdministratorId",
                schema: "payment",
                table: "WalletTransactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
