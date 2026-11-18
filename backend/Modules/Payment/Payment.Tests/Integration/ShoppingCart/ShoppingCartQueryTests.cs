using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.ShoppingCart;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.ShoppingCart;

public class ShoppingCartQueryTests : BaseIntegrationTest
{
    public ShoppingCartQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task The_cart_shows_only_the_signed_in_explorers_items_and_total()
    {
        var seed = ShoppingCartSeed.ExplorerCart;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var cart = await client.GetFromJsonAsync<ShoppingCartDto>("/api/payment/cart", JsonOptions);

        cart.Should().NotBeNull();
        cart!.Items.Should().BeEquivalentTo(seed.Items);
        cart.TotalInEc.Should().Be(seed.TotalInEc);
    }

    [Fact]
    public async Task An_explorer_without_a_cart_sees_an_empty_cart()
    {
        using var client = Factory.CreateClientFor(ShoppingCartSeed.ExplorerWithoutCartId, "explorer");

        var cart = await client.GetFromJsonAsync<ShoppingCartDto>("/api/payment/cart", JsonOptions);

        cart.Should().NotBeNull();
        cart!.Items.Should().BeEmpty();
        cart.TotalInEc.Should().Be(0);
    }

    [Fact]
    public async Task A_tour_can_be_previewed_before_adding_it()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var product = ProductSeed.FreeWalk;

        var preview = await client.GetFromJsonAsync<ProductForCartDto>(
            $"/api/payment/cart/products/{product.TourId}", JsonOptions);

        preview.Should().Be(new ProductForCartDto(product.TourId, product.Name, product.PriceInEc));
    }

    [Fact]
    public async Task A_tour_without_a_product_cannot_be_previewed()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/payment/cart/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
