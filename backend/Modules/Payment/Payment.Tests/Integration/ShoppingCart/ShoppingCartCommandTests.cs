using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.ShoppingCart;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.ShoppingCart;

public class ShoppingCartCommandTests : BaseIntegrationTest
{
    private const string CouponsUrl = "/api/payment/cart/coupons";

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

    [Fact]
    public async Task A_code_typed_in_lowercase_enters_the_coupon_into_the_buyers_cart()
    {
        var seed = ShoppingCartSeed.ExplorerCart;
        var coupon = CouponSeed.AuthorCityWalkCoupon;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.PostAsJsonAsync(CouponsUrl, new EnterCouponDto(coupon.Code.Value.ToLowerInvariant()));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.CouponIds.Should().Equal(coupon.Id);
    }

    [Fact]
    public async Task An_unknown_code_leaves_the_cart_unchanged()
    {
        var seed = ShoppingCartSeed.ExplorerCart;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.PostAsJsonAsync(CouponsUrl, new EnterCouponDto("NOSUCH00"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.CouponIds.Should().Equal(seed.CouponIds);
    }

    [Fact]
    public async Task A_coupon_used_in_a_completed_purchase_cannot_be_entered_again()
    {
        var seed = ShoppingCartSeed.CartOfBuyerWhoUsedGeneralCoupon;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.PostAsJsonAsync(
            CouponsUrl, new EnterCouponDto(CouponSeed.AuthorGeneralCoupon.Code.Value));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.CouponIds.Should().Equal(seed.CouponIds);
    }

    [Fact]
    public async Task A_removed_coupon_is_no_longer_in_the_cart()
    {
        var seed = ShoppingCartSeed.CartWithWinningAndLosingCoupon;
        var removed = CouponSeed.AuthorCityWalkCoupon;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.DeleteAsync(CouponUrl(removed.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.CouponIds.Should().Equal(seed.CouponIds.Where(id => id != removed.Id));
    }

    [Fact]
    public async Task A_coupon_that_is_not_in_the_cart_cannot_be_removed()
    {
        var seed = ShoppingCartSeed.CartWithWinningAndLosingCoupon;
        using var client = Factory.CreateClientFor(seed.UserId, "explorer");

        var response = await client.DeleteAsync(CouponUrl(CouponSeed.OtherAuthorRiverWalkCoupon.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(row => row.Id == seed.Id);
        cart.CouponIds.Should().Equal(seed.CouponIds);
    }

    [Theory]
    [InlineData("POST", "items", null, HttpStatusCode.Unauthorized)]
    [InlineData("DELETE", "items", null, HttpStatusCode.Unauthorized)]
    [InlineData("POST", "coupons", null, HttpStatusCode.Unauthorized)]
    [InlineData("DELETE", "coupons", null, HttpStatusCode.Unauthorized)]
    [InlineData("POST", "items", "administrator", HttpStatusCode.Forbidden)]
    [InlineData("DELETE", "items", "administrator", HttpStatusCode.Forbidden)]
    [InlineData("POST", "coupons", "administrator", HttpStatusCode.Forbidden)]
    [InlineData("DELETE", "coupons", "administrator", HttpStatusCode.Forbidden)]
    public async Task Only_authenticated_explorers_can_change_a_cart(
        string method, string part, string? role, HttpStatusCode expected)
    {
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Administrator, role);
        var url = (method, part) switch
        {
            (_, "items") => ItemUrl(ProductSeed.CityWalk.TourId),
            ("POST", _) => CouponsUrl,
            _ => CouponUrl(CouponSeed.AuthorCityWalkCoupon.Id)
        };
        using var request = new HttpRequestMessage(new HttpMethod(method), url)
        {
            Content = JsonContent.Create(new EnterCouponDto(CouponSeed.AuthorCityWalkCoupon.Code.Value))
        };

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(expected);
    }

    private static string ItemUrl(Guid tourId) => $"/api/payment/cart/items/{tourId}";

    private static string CouponUrl(Guid couponId) => $"{CouponsUrl}/{couponId}";

}
