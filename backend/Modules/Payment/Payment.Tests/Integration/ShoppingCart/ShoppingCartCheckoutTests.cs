using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.ShoppingCart;
using Payment.Domain.GiftCards;
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

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc));

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

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc));

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

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc));

        await AssertRejectedAsync(response);
        await AssertCheckoutUnchangedAsync(cart, wallet);
    }

    [Fact]
    public async Task A_tour_the_buyer_already_owns_cannot_be_purchased_again()
    {
        var cart = ShoppingCartSeed.AlreadyPurchasedCart;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc));

        await AssertRejectedAsync(response);
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.ExplorerWalletWithReversal);
    }

    [Fact]
    public async Task One_archived_tour_blocks_the_entire_purchase()
    {
        var cart = ShoppingCartSeed.ArchivedCheckoutCart;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc));

        await AssertRejectedAsync(response);
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.DemoExplorerWalletWithDeposit);
    }

    [Fact]
    public async Task Buying_with_a_coupon_charges_the_discounted_total_and_records_the_coupon_and_discount()
    {
        var cart = ShoppingCartSeed.CartWithWinningAndLosingCoupon;
        var wallet = WalletSeed.CouponBuyerWalletWithDeposit;
        var discounted = ProductSeed.CityWalk;
        var coupon = CouponSeed.AuthorGeneralCoupon;
        var discount = discounted.PriceInEc * coupon.DiscountPercent / 100;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc - discount));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var savedWallet = await context.Wallets.Include(row => row.Transactions).SingleAsync(row => row.Id == wallet.Id);
        savedWallet.Balance.Should().Be(wallet.Balance - (int)cart.TotalInEc + discount);
        savedWallet.Transactions.Where(row => row.Kind == WalletTransactionKind.Purchase)
            .Select(row => new { row.TourId, row.Amount, row.DiscountInEc })
            .Should().BeEquivalentTo(cart.Items.Select(item => item.TourId == discounted.TourId
                ? new { TourId = (Guid?)item.TourId, Amount = -(item.PriceInEc - discount), DiscountInEc = discount }
                : new { TourId = (Guid?)item.TourId, Amount = -item.PriceInEc, DiscountInEc = 0 }));
        var tokens = await context.TourPurchaseTokens.Where(row => row.BuyerId == cart.UserId).ToListAsync();
        tokens.Select(row => new { row.TourId, row.CouponId, row.DiscountInEc }).Should().BeEquivalentTo(
            cart.Items.Select(item => item.TourId == discounted.TourId
                ? new { item.TourId, CouponId = (Guid?)coupon.Id, DiscountInEc = discount }
                : new { item.TourId, CouponId = (Guid?)null, DiscountInEc = 0 }));
        var savedCart = await context.ShoppingCarts.SingleAsync(row => row.Id == cart.Id);
        savedCart.Items.Should().BeEmpty();
        savedCart.CouponIds.Should().BeEmpty();
    }

    [Fact]
    public async Task A_purchase_with_a_total_that_no_longer_matches_leaves_the_database_intact()
    {
        var cart = ShoppingCartSeed.CartWithWinningAndLosingCoupon;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc));

        await AssertRejectedAsync(response);
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.CouponBuyerWalletWithDeposit);
    }

    [Fact]
    public async Task Buying_with_a_gift_card_charges_only_the_rest_spends_the_card_and_records_its_part()
    {
        var cart = ShoppingCartSeed.CartWithGiftCard;
        var wallet = WalletSeed.GiftCardBuyerWalletWithDeposit;
        var giftCard = GiftCardSeed.CardInGiftCardBuyerCart;
        var coveredInFull = ProductSeed.CityWalk;
        var coveredInPart = ProductSeed.MuseumWalk;
        var partOfSecond = giftCard.Amount - coveredInFull.PriceInEc;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc - giftCard.Amount));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var savedWallet = await context.Wallets.Include(row => row.Transactions).SingleAsync(row => row.Id == wallet.Id);
        savedWallet.Balance.Should().Be(wallet.Balance - (int)cart.TotalInEc + giftCard.Amount);
        savedWallet.Transactions.Where(row => row.Kind == WalletTransactionKind.Purchase)
            .Select(row => new { row.TourId, row.Amount, row.GiftCardInEc })
            .Should().BeEquivalentTo(new[]
            {
                new { TourId = (Guid?)coveredInFull.TourId, Amount = 0, GiftCardInEc = coveredInFull.PriceInEc },
                new
                {
                    TourId = (Guid?)coveredInPart.TourId,
                    Amount = -(coveredInPart.PriceInEc - partOfSecond),
                    GiftCardInEc = partOfSecond
                }
            });
        var savedGiftCard = await context.GiftCards.SingleAsync(row => row.Id == giftCard.Id);
        savedGiftCard.StatusAt(DateTime.UtcNow).Should().Be(GiftCardStatus.Redeemed);
        (await context.ShoppingCarts.SingleAsync(row => row.Id == cart.Id)).GiftCardId.Should().BeNull();
    }

    [Fact]
    public async Task A_purchase_with_a_gift_card_that_expired_meanwhile_leaves_the_card_and_wallet_intact()
    {
        var cart = ShoppingCartSeed.CartWithGiftCardThatExpired;
        var giftCard = GiftCardSeed.CardExpiredAfterEnteringIntoCart;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc - giftCard.Amount));

        await AssertRejectedAsync(response);
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.ExpiredGiftCardBuyerWalletWithDeposit);
        await AssertGiftCardNotRedeemedAsync(giftCard);
    }

    [Fact]
    public async Task A_purchase_that_fails_for_lack_of_ec_for_the_rest_leaves_the_applied_gift_card_valid()
    {
        var cart = ShoppingCartSeed.UnfundedCartWithGiftCard;
        var giftCard = GiftCardSeed.CardInUnfundedGiftCardBuyerCart;
        using var client = Factory.CreateClientFor(cart.UserId, "explorer");

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc - giftCard.Amount));

        await AssertRejectedAsync(response);
        await AssertCheckoutUnchangedAsync(cart, WalletSeed.UnfundedGiftCardBuyerWalletWithDeposit);
        await AssertGiftCardNotRedeemedAsync(giftCard);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_authenticated_explorers_can_checkout(string? role, HttpStatusCode expected)
    {
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        var response = await client.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(0));

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
            firstClient.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc)),
            secondClient.PostAsJsonAsync(CheckoutUrl, new CheckoutDto(cart.TotalInEc)));

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
        savedCart.CouponIds.Should().Equal(cart.CouponIds);
        savedCart.GiftCardId.Should().Be(cart.GiftCardId);
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

    private async Task AssertGiftCardNotRedeemedAsync(GiftCard giftCard)
    {
        using var context = Factory.CreateContext<PaymentDbContext>();
        var savedGiftCard = await context.GiftCards.SingleAsync(row => row.Id == giftCard.Id);
        savedGiftCard.RedeemedAt.Should().BeNull();
    }

}
