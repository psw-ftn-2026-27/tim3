using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.CouponAuthoring;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CouponAuthoring;

public class CouponAuthoringQueryTests : BaseIntegrationTest
{
    public CouponAuthoringQueryTests(PaymentApiFactory factory) : base(factory)
    {
        Factory.Reseed<DbContext>(CheckoutTourSeed.All);
    }

    [Fact]
    public async Task Author_sees_only_their_own_coupons_newest_first()
    {
        var authorId = WellKnownUsers.ThirdExplorer;
        var expectedIds = CouponSeed.All
            .Where(coupon => coupon.AuthorId == authorId)
            .OrderByDescending(coupon => coupon.CreatedAt)
            .Select(coupon => coupon.Id);
        using var client = Factory.CreateClientFor(authorId, "explorer");

        var coupons = await client.GetFromJsonAsync<List<CouponDto>>("/api/payment/coupons", JsonOptions);

        coupons!.Select(coupon => coupon.Id).Should().Equal(expectedIds);
    }

    [Fact]
    public async Task Coupons_past_their_last_valid_day_are_marked_as_expired()
    {
        var authorId = WellKnownUsers.ThirdExplorer;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expected = CouponSeed.All
            .Where(coupon => coupon.AuthorId == authorId)
            .ToDictionary(coupon => coupon.Id, coupon => coupon.IsExpiredOn(today));
        using var client = Factory.CreateClientFor(authorId, "explorer");

        var coupons = await client.GetFromJsonAsync<List<CouponDto>>("/api/payment/coupons", JsonOptions);

        coupons!.ToDictionary(coupon => coupon.Id, coupon => coupon.IsExpired).Should().Equal(expected);
    }

    [Fact]
    public async Task Products_for_a_coupon_exclude_free_products_tours_not_on_offer_and_other_authors_products()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.ThirdExplorer, "explorer");

        var products = await client.GetFromJsonAsync<List<ProductForCouponDto>>(
            "/api/payment/coupons/products", JsonOptions);

        var ids = products!.Select(product => product.Id).ToList();
        ids.Should().Contain([ProductSeed.CityWalk.Id, ProductSeed.MuseumWalk.Id]);
        ids.Should().NotContain(
            [ProductSeed.FreeWalk.Id, ProductSeed.ArchivedWalk.Id, ProductSeed.OtherAuthorRiverWalk.Id]);
    }
}
