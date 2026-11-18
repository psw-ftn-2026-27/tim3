using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.CouponAuthoring;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CouponAuthoring;

public class CouponAuthoringCommandTests : BaseIntegrationTest
{
    private const string CouponsUrl = "/api/payment/coupons";
    private static readonly DateOnly ValidUntil = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);

    public CouponAuthoringCommandTests(PaymentApiFactory factory) : base(factory)
    {
        Factory.Reseed<DbContext>(CheckoutTourSeed.All);
    }

    [Fact]
    public async Task Author_creates_a_coupon_for_their_product_that_is_stored_with_them_as_author()
    {
        var product = ProductSeed.MuseumWalk;
        using var client = Factory.CreateClientFor(product.AuthorId, "explorer");
        var coupon = new CreateCouponDto(20, ValidUntil, product.Id);
        var couponsBefore = await CountCouponsAsync(product.AuthorId);

        var response = await client.PostAsJsonAsync(CouponsUrl, coupon);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var stored = await context.Coupons.Where(row => row.AuthorId == product.AuthorId).ToListAsync();
        stored.Should().HaveCount(couponsBefore + 1);
        stored.Should().ContainSingle(row => row.ProductId == product.Id
            && row.DiscountPercent == coupon.DiscountPercent
            && row.ValidUntil == coupon.ValidUntil);
    }

    [Fact]
    public async Task A_coupon_for_another_authors_product_is_not_stored()
    {
        var authorId = ProductSeed.MuseumWalk.AuthorId;
        using var client = Factory.CreateClientFor(authorId, "explorer");
        var couponsBefore = await CountCouponsAsync(authorId);

        var response = await client.PostAsJsonAsync(
            CouponsUrl, new CreateCouponDto(20, ValidUntil, ProductSeed.OtherAuthorRiverWalk.Id));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await CountCouponsAsync(authorId)).Should().Be(couponsBefore);
    }

    [Fact]
    public async Task A_coupon_for_a_tour_that_is_not_on_offer_is_not_stored()
    {
        var product = ProductSeed.ArchivedWalk;
        using var client = Factory.CreateClientFor(product.AuthorId, "explorer");
        var couponsBefore = await CountCouponsAsync(product.AuthorId);

        var response = await client.PostAsJsonAsync(CouponsUrl, new CreateCouponDto(20, ValidUntil, product.Id));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await CountCouponsAsync(product.AuthorId)).Should().Be(couponsBefore);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_explorers_can_create_a_coupon(string? role, HttpStatusCode expected)
    {
        var product = ProductSeed.MuseumWalk;
        using var client = role is null
            ? Factory.CreateClient()
            : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        var response = await client.PostAsJsonAsync(CouponsUrl, new CreateCouponDto(20, ValidUntil, product.Id));

        response.StatusCode.Should().Be(expected);
    }

    private async Task<int> CountCouponsAsync(Guid authorId)
    {
        using var context = Factory.CreateContext<PaymentDbContext>();
        return await context.Coupons.CountAsync(row => row.AuthorId == authorId);
    }
}
