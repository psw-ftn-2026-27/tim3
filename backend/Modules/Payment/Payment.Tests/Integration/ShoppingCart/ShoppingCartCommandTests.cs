using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.ShoppingCart;

public class ShoppingCartCommandTests : BaseIntegrationTest
{
    public ShoppingCartCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Adding_a_free_tour_creates_the_first_cart()
    {
        var userId = ShoppingCartSeed.ExplorerWithoutCartId;
        using var client = Factory.CreateClientFor(userId, "explorer");
        var product = ProductSeed.FreeWalk;

        var response = await client.PostAsync(ItemUrl(product.TourId), null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.UserId == userId);
        var item = cart.Items.Should().ContainSingle().Which;
        item.ProductId.Should().Be(product.Id);
        item.TourId.Should().Be(product.TourId);
        item.Name.Should().Be(product.Name);
        item.PriceInEc.Should().Be(0);
    }

    [Fact]
    public async Task Adding_to_an_existing_cart_keeps_the_previous_items()
    {
        var seed = ShoppingCartSeed.OtherExplorerCart;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");
        var product = ProductSeed.CityWalk;

        var response = await client.PostAsync(ItemUrl(product.TourId), null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.Items.Should().Contain(seed.Items.Single());
        cart.Items.Should().Contain(item => item.TourId == product.TourId);
        cart.TotalInEc.Should().Be(seed.TotalInEc + product.PriceInEc);
    }

    [Fact]
    public async Task Removing_the_last_tour_leaves_an_empty_cart()
    {
        var seed = ShoppingCartSeed.ExplorerCart;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.DeleteAsync(ItemUrl(ProductSeed.CityWalk.TourId));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task A_tour_cannot_be_removed_from_another_explorers_cart()
    {
        var seed = ShoppingCartSeed.OtherExplorerCart;
        var tourId = seed.Items.Single().TourId;
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync(ItemUrl(tourId));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.Items.Should().BeEquivalentTo(seed.Items);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_duplicate_or_owned_tour_leaves_the_cart_intact(bool alreadyPurchased)
    {
        var seed = ShoppingCartSeed.ExplorerCart;
        var tourId = alreadyPurchased ? ProductSeed.MuseumWalk.TourId : ProductSeed.CityWalk.TourId;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.PostAsync(ItemUrl(tourId), null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.Items.Should().BeEquivalentTo(seed.Items);
    }

    [Fact]
    public async Task A_tour_without_a_product_creates_no_cart()
    {
        var userId = ShoppingCartSeed.ExplorerWithoutCartId;
        using var client = Factory.CreateClientFor(userId, "explorer");

        var response = await client.PostAsync(ItemUrl(Guid.NewGuid()), null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        context.ShoppingCarts.Should().NotContain(cart => cart.UserId == userId);
    }

    [Theory]
    [InlineData("POST", null, HttpStatusCode.Unauthorized)]
    [InlineData("DELETE", null, HttpStatusCode.Unauthorized)]
    [InlineData("POST", "administrator", HttpStatusCode.Forbidden)]
    [InlineData("DELETE", "administrator", HttpStatusCode.Forbidden)]
    public async Task Only_authenticated_explorers_can_change_a_cart(
        string method, string? role, HttpStatusCode expected)
    {
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Administrator, role);
        using var request = new HttpRequestMessage(new HttpMethod(method), ItemUrl(ProductSeed.CityWalk.TourId));

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(expected);
    }

    private static string ItemUrl(Guid tourId) => $"/api/payment/cart/items/{tourId}";
}
