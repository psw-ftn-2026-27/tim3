using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;
using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;

namespace Payment.Tests.Integration.ShoppingCart;

public sealed class ShoppingCartCheckoutTests : BaseIntegrationTest
{
    private const string CheckoutUrl = "/api/payment/cart/checkout";

    public ShoppingCartCheckoutTests(PaymentApiFactory factory) : base(factory)
    {
        Factory.Reseed<DbContext>(CheckoutTourSeed.All);
    }

    [Fact]
    public async Task Buying_paid_and_free_tours_persists_every_effect()
    {
        var cart = ShoppingCartSeed.FundedCheckoutCart;
        var wallet = WalletSeed.ExplorerWalletWithDeposit;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var savedCart = await context.ShoppingCarts.SingleAsync(row => row.Id == cart.Id);
        savedCart.Items.Should().BeEmpty();
        var savedWallet = await context.Wallets.Include(row => row.Transactions).SingleAsync(row => row.Id == wallet.Id);
        savedWallet.Balance.Should().Be(wallet.Balance - (int)cart.TotalInEc);
        savedWallet.Transactions.Should().Contain(wallet.Transactions.Single());
        var purchases = savedWallet.Transactions.Where(row => row.Kind == WalletTransactionKind.Purchase).ToList();
        purchases.Select(row => new { row.TourId, row.Amount }).Should().BeEquivalentTo(
            cart.Items.Select(item => new { TourId = (Guid?)item.TourId, Amount = -item.PriceInEc }));
        var tokens = await context.TourPurchaseTokens.Where(row => row.BuyerId == cart.UserId).ToListAsync();
        tokens.Select(row => row.TourId).Should().BeEquivalentTo(cart.Items.Select(item => item.TourId));
        tokens.Should().OnlyContain(token =>
            purchases.Any(row => row.TourId == token.TourId && row.OccurredAt == token.PurchasedAt));

    }

    [Fact]
    public async Task A_free_purchase_without_a_wallet_records_zero_and_grants_ownership()
    {
        var cart = ShoppingCartSeed.FreeCheckoutCart;
        var product = ProductSeed.FreeWalk;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(row => row.Transactions).SingleAsync(row => row.UserId == cart.UserId);
        wallet.Balance.Should().Be(0);
        var purchase = wallet.Transactions.Should().ContainSingle().Which;
        purchase.Kind.Should().Be(WalletTransactionKind.Purchase);
        purchase.Amount.Should().Be(0);
        purchase.TourId.Should().Be(product.TourId);
        var token = await context.TourPurchaseTokens.SingleAsync(row => row.BuyerId == cart.UserId);
        token.TourId.Should().Be(product.TourId);
        (await context.ShoppingCarts.SingleAsync(row => row.Id == cart.Id)).Items.Should().BeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unfunded_purchase_leaves_the_database_intact(bool hasWallet)
    {
        var cart = hasWallet ? ShoppingCartSeed.ExplorerCart : ShoppingCartSeed.OtherExplorerCart;
        var wallet = hasWallet ? WalletSeed.EmptyExplorerWallet : null;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        await AssertRejectedAsync(response);
        await AssertCheckoutUnchangedAsync(cart, wallet);
    }

    [Fact]
    public async Task A_tour_the_buyer_already_owns_cannot_be_purchased_again()
    {
        var cart = ShoppingCartSeed.AlreadyPurchasedCart;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        await AssertRejectedAsync(response);
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.ExplorerWalletWithReversal);
    }

    [Fact]
    public async Task One_archived_tour_blocks_the_entire_purchase()
    {
        var cart = ShoppingCartSeed.ArchivedCheckoutCart;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsync(CheckoutUrl, null);

        await AssertRejectedAsync(response);
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.DemoExplorerWalletWithDeposit);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_authenticated_explorers_can_checkout(string? role, HttpStatusCode expected)
    {
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        var response = await client.PostAsync(CheckoutUrl, null);

        response.StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task Simultaneous_purchases_charge_the_wallet_only_once()
    {
        var cart = ShoppingCartSeed.FundedCheckoutCart;
        var wallet = WalletSeed.ExplorerWalletWithDeposit;
        using var firstClient = Factory.CreateClientFor(cart.UserId, "explorer");
        using var secondClient = Factory.CreateClientFor(cart.UserId, "explorer");

        var responses = await Task.WhenAll(
            firstClient.PostAsync(CheckoutUrl, null),
            secondClient.PostAsync(CheckoutUrl, null));

        responses.Select(response => response.StatusCode).Should().BeEquivalentTo(
            [HttpStatusCode.NoContent, HttpStatusCode.BadRequest]);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var savedWallet = await context.Wallets.Include(row => row.Transactions)
            .SingleAsync(row => row.Id == wallet.Id);
        savedWallet.Balance.Should().Be(wallet.Balance - (int)cart.TotalInEc);
        savedWallet.Transactions.Where(row => row.Kind == WalletTransactionKind.Purchase)
            .Select(row => new { row.TourId, row.Amount }).Should().BeEquivalentTo(
                cart.Items.Select(item => new { TourId = (Guid?)item.TourId, Amount = -item.PriceInEc }));
        var tokens = await context.TourPurchaseTokens.Where(row => row.BuyerId == cart.UserId).ToListAsync();
        tokens.Select(token => token.TourId).Should().BeEquivalentTo(cart.Items.Select(item => item.TourId));
        (await context.ShoppingCarts.SingleAsync(row => row.Id == cart.Id)).Items.Should().BeEmpty();
    }
    private static Task AssertRejectedAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        return Task.CompletedTask;
    }

    private async Task AssertCheckoutUnchangedAsync(Cart cart, Wallet? wallet)
    {
        using var context = Factory.CreateContext<PaymentDbContext>();
        var savedCart = await context.ShoppingCarts.SingleAsync(row => row.Id == cart.Id);
        savedCart.Items.Should().BeEquivalentTo(cart.Items);
        var savedWallet = await context.Wallets.Include(row => row.Transactions)
            .SingleOrDefaultAsync(row => row.UserId == cart.UserId);
        if (wallet is null)
        {
            savedWallet.Should().BeNull();
        }
        else
        {
            savedWallet.Should().NotBeNull();
            savedWallet!.Balance.Should().Be(wallet.Balance);
            savedWallet.Transactions.Should().BeEquivalentTo(wallet.Transactions);
        }

        var tokens = await context.TourPurchaseTokens.Where(row => row.BuyerId == cart.UserId).ToListAsync();
        tokens.Should().BeEquivalentTo(TourPurchaseTokenSeed.All.Where(row => row.BuyerId == cart.UserId));
    }

}
