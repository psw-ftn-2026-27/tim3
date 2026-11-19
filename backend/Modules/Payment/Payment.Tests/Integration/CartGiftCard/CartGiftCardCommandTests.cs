using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CartGiftCard;

public class CartGiftCardCommandTests : BaseIntegrationTest
{
    private const string GiftCardUrl = "/api/payment/cart/gift-card";

    public CartGiftCardCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task A_buyer_chooses_their_valid_gift_card_and_it_stays_in_their_cart()
    {
        var seed = ShoppingCartSeed.OtherExplorerCart;
        var giftCard = GiftCardSeed.DemoExplorerCardForSecondDemoExplorer;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.PostAsync(EnterGiftCardUrl(giftCard.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.GiftCardId.Should().Be(giftCard.Id);
    }

    [Fact]
    public async Task Choosing_a_gift_card_creates_the_first_cart()
    {
        var userId = ShoppingCartSeed.ExplorerWithoutCartId;
        var giftCard = GiftCardSeed.DemoExplorerCardForThirdDemoExplorer;
        using var client = Factory.CreateClientFor(userId, "explorer");

        var response = await client.PostAsync(EnterGiftCardUrl(giftCard.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.UserId == userId);
        cart.GiftCardId.Should().Be(giftCard.Id);
    }

    [Fact]
    public async Task An_unknown_gift_card_leaves_the_cart_unchanged()
    {
        var seed = ShoppingCartSeed.OtherExplorerCart;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.PostAsync(EnterGiftCardUrl(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.GiftCardId.Should().Be(seed.GiftCardId);
    }

    [Fact]
    public async Task A_gift_card_received_by_another_buyer_cannot_be_chosen()
    {
        var seed = ShoppingCartSeed.OtherExplorerCart;
        var othersGiftCard = GiftCardSeed.SecondDemoExplorerCardForDemoExplorer;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.PostAsync(EnterGiftCardUrl(othersGiftCard.Id), null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.GiftCardId.Should().Be(seed.GiftCardId);
    }

    [Fact]
    public async Task A_removed_gift_card_is_no_longer_in_the_cart()
    {
        var seed = ShoppingCartSeed.CartWithGiftCard;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.DeleteAsync(GiftCardUrl);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.GiftCardId.Should().BeNull();
    }

    [Fact]
    public async Task A_gift_card_cannot_be_removed_from_a_cart_without_one()
    {
        var seed = ShoppingCartSeed.OtherExplorerCart;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.DeleteAsync(GiftCardUrl);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.GiftCardId.Should().Be(seed.GiftCardId);
    }

    [Theory]
    [InlineData("POST", null, HttpStatusCode.Unauthorized)]
    [InlineData("DELETE", null, HttpStatusCode.Unauthorized)]
    [InlineData("POST", "administrator", HttpStatusCode.Forbidden)]
    [InlineData("DELETE", "administrator", HttpStatusCode.Forbidden)]
    public async Task Only_authenticated_explorers_can_choose_or_remove_a_gift_card(
        string method, string? role, HttpStatusCode expected)
    {
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Administrator, role);
        var url = method == "POST"
            ? EnterGiftCardUrl(GiftCardSeed.DemoExplorerCardForSecondDemoExplorer.Id)
            : GiftCardUrl;
        using var request = new HttpRequestMessage(new HttpMethod(method), url);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(expected);
    }

    private static string EnterGiftCardUrl(Guid giftCardId) => $"{GiftCardUrl}/{giftCardId}";
}
