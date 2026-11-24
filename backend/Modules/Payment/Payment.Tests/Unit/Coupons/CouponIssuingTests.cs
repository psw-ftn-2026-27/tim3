using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Coupons;

public class CouponIssuingTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);
    private static readonly Guid Author = WellKnownUsers.ThirdExplorer;

    [Fact]
    public void A_coupon_for_an_own_paid_product_on_offer_is_issued_for_that_product()
    {
        var product = new Product(Guid.NewGuid(), "City walk", Author, 25);
        HashSet<Guid> available = [product.TourId];

        var coupon = Issue([product], available, product.Id);

        coupon.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public void A_general_coupon_is_issued_when_the_author_has_a_paid_product_on_offer()
    {
        var freeProduct = new Product(Guid.NewGuid(), "Free walk", Author, 0);
        var paidProduct = new Product(Guid.NewGuid(), "City walk", Author, 25);
        HashSet<Guid> available = [freeProduct.TourId, paidProduct.TourId];

        var coupon = Issue([freeProduct, paidProduct], available, null);

        coupon.ProductId.Should().BeNull();
    }

    [Fact]
    public void A_coupon_for_another_authors_product_is_rejected()
    {
        var ownProduct = new Product(Guid.NewGuid(), "City walk", Author, 25);
        var otherProduct = new Product(Guid.NewGuid(), "River walk", WellKnownUsers.FourthExplorer, 20);
        HashSet<Guid> available = [ownProduct.TourId, otherProduct.TourId];

        var issue = () => Issue([ownProduct, otherProduct], available, otherProduct.Id);

        issue.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void A_general_coupon_is_rejected_for_an_author_without_a_paid_product_on_offer()
    {
        var freeProduct = new Product(Guid.NewGuid(), "Free walk", Author, 0);
        var archivedProduct = new Product(Guid.NewGuid(), "Archived walk", Author, 10);
        HashSet<Guid> available = [freeProduct.TourId];

        var issue = () => Issue([freeProduct, archivedProduct], available, null);

        issue.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_coupon_for_a_free_product_is_rejected()
    {
        var freeProduct = new Product(Guid.NewGuid(), "Free walk", Author, 0);
        HashSet<Guid> available = [freeProduct.TourId];

        var issue = () => Issue([freeProduct], available, freeProduct.Id);

        issue.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_coupon_for_a_tour_that_is_not_on_offer_is_rejected()
    {
        var archivedProduct = new Product(Guid.NewGuid(), "Archived walk", Author, 10);

        var issue = () => Issue([archivedProduct], new HashSet<Guid>(), archivedProduct.Id);

        issue.Should().Throw<DomainException>();
    }

    private static Coupon Issue(Product[] products, HashSet<Guid> availableTourIds, Guid? productId)
    {
        return new CouponIssuing().Issue(
            Author, products, availableTourIds, new CouponRequest(productId, 10, Today.AddDays(10)), Now);
    }
}
