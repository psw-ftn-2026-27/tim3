using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCardPurchasing;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration.GiftCardPurchasing;

public class GiftCardPurchasingCommandTests : BaseIntegrationTest
{
    public GiftCardPurchasingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Buyer_buys_a_gift_card_that_is_stored_for_the_recipient_and_paid_from_the_wallet()
    {
        var wallet = WalletSeed.DemoExplorerWalletWithDeposit;
        var recipientId = GiftCardSeed.SecondDemoExplorerId;
        using var client = Factory.CreateClientFor(wallet.UserId, "explorer");
        var purchase = new BuyGiftCardDto(recipientId, 25, "Enjoy the tour");
        int cardsBefore;
        using (var before = Factory.CreateContext<PaymentDbContext>())
        {
            cardsBefore = await before.GiftCards.CountAsync(card => card.BuyerId == wallet.UserId);
        }

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards", purchase);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cards = await context.GiftCards.Where(card => card.BuyerId == wallet.UserId).ToListAsync();
        cards.Should().HaveCount(cardsBefore + 1);
        cards.Should().ContainSingle(card => card.RecipientId == recipientId
            && card.Amount == purchase.Amount
            && card.Message == purchase.Message);
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance - purchase.Amount);
        stored.Transactions.Should().ContainSingle(transaction => transaction.Kind == WalletTransactionKind.GiftCardPurchase)
            .Which.Amount.Should().Be(-purchase.Amount);
    }

    [Fact]
    public async Task Buying_for_a_recipient_who_does_not_exist_stores_no_card_and_keeps_the_balance()
    {
        var wallet = WalletSeed.DemoExplorerWalletWithDeposit;
        using var client = Factory.CreateClientFor(wallet.UserId, "explorer");
        var unknownRecipientId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards", new BuyGiftCardDto(unknownRecipientId, 25, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.GiftCards.AnyAsync(card => card.RecipientId == unknownRecipientId)).Should().BeFalse();
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance);
        stored.Transactions.Should().BeEquivalentTo(wallet.Transactions);
    }

    [Fact]
    public async Task Buying_for_a_user_who_is_not_an_explorer_stores_no_card_and_keeps_the_balance()
    {
        var wallet = WalletSeed.DemoExplorerWalletWithDeposit;
        using var client = Factory.CreateClientFor(wallet.UserId, "explorer");
        var administratorId = GiftCardSeed.DemoAdministratorId;

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards", new BuyGiftCardDto(administratorId, 25, null));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.GiftCards.AnyAsync(card => card.RecipientId == administratorId)).Should().BeFalse();
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance);
        stored.Transactions.Should().BeEquivalentTo(wallet.Transactions);
    }

    [Fact]
    public async Task Simultaneous_purchases_never_spend_more_than_the_wallet_holds()
    {
        var wallet = WalletSeed.DemoExplorerWalletWithDeposit;
        using var client = Factory.CreateClientFor(wallet.UserId, "explorer");
        var amount = wallet.Balance / 2 + 1;
        var purchase = new BuyGiftCardDto(GiftCardSeed.SecondDemoExplorerId, amount, null);
        int cardsBefore;
        using (var before = Factory.CreateContext<PaymentDbContext>())
        {
            cardsBefore = await before.GiftCards.CountAsync(card => card.BuyerId == wallet.UserId);
        }

        var responses = await Task.WhenAll(
            client.PostAsJsonAsync("/api/payment/gift-cards", purchase),
            client.PostAsJsonAsync("/api/payment/gift-cards", purchase));

        responses.Select(response => response.StatusCode).Should()
            .BeEquivalentTo([HttpStatusCode.NoContent, HttpStatusCode.BadRequest]);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.GiftCards.CountAsync(card => card.BuyerId == wallet.UserId)).Should().Be(cardsBefore + 1);
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance - amount);
        stored.Transactions.Should().ContainSingle(transaction => transaction.Kind == WalletTransactionKind.GiftCardPurchase);
    }

    [Fact]
    public async Task Buying_without_enough_ec_stores_no_card_and_keeps_the_balance()
    {
        var wallet = WalletSeed.EmptyExplorerWallet;
        using var client = Factory.CreateClientFor(wallet.UserId, "explorer");

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards", new BuyGiftCardDto(GiftCardSeed.SecondDemoExplorerId, 10, null));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.GiftCards.AnyAsync(card => card.BuyerId == wallet.UserId)).Should().BeFalse();
        var stored = await context.Wallets.Include(item => item.Transactions).SingleAsync(item => item.Id == wallet.Id);
        stored.Balance.Should().Be(wallet.Balance);
        stored.Transactions.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_explorers_can_buy_a_gift_card(string? role, HttpStatusCode expected)
    {
        var buyerId = WalletSeed.DemoExplorerWalletWithDeposit.UserId;
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(buyerId, role);

        var response = await client.PostAsJsonAsync(
            "/api/payment/gift-cards", new BuyGiftCardDto(GiftCardSeed.SecondDemoExplorerId, 25, null));

        response.StatusCode.Should().Be(expected);
    }
}
