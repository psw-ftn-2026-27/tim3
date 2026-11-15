using FluentAssertions;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.GiftCards;

public class GiftCardPurchaseTests
{
    private static readonly DateTime PurchaseTime = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void A_purchase_creates_a_card_with_exactly_the_amount_charged_from_the_wallet()
    {
        var wallet = new Wallet(WellKnownUsers.Explorer);
        wallet.Deposit(100, WellKnownUsers.Administrator, PurchaseTime.AddHours(-1));

        var giftCard = GiftCardPurchase.Buy(wallet, WellKnownUsers.SecondExplorer, 30, "Enjoy", PurchaseTime);

        wallet.Balance.Should().Be(100 - giftCard.Amount);
        giftCard.Amount.Should().Be(30);
        giftCard.BuyerId.Should().Be(wallet.UserId);
        giftCard.RecipientId.Should().Be(WellKnownUsers.SecondExplorer);
        wallet.Transactions.Should().ContainSingle(transaction => transaction.Kind == WalletTransactionKind.GiftCardPurchase)
            .Which.Amount.Should().Be(-giftCard.Amount);
    }
}
