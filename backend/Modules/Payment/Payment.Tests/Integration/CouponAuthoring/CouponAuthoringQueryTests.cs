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
    public async Task Author_sees_only_their_own_coupons()
    {
        var authorId = WellKnownUsers.ThirdExplorer;
        var expectedIds = CouponSeed.All.Where(coupon => coupon.AuthorId == authorId).Select(coupon => coupon.Id);
        using var client = Factory.CreateClientFor(authorId, "explorer");

        var coupons = await client.GetFromJsonAsync<List<CouponDto>>("/api/payment/coupons", JsonOptions);

        coupons!.Select(coupon => coupon.Id).Should().BeEquivalentTo(expectedIds);
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
