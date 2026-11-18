using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public class CouponEntryTests
{
    private static readonly DateTime CreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(CreatedAt);
    private static readonly Guid Author = Guid.NewGuid();

    [Theory]
    [InlineData("expired")]
    [InlineData("used")]
    [InlineData("other author")]
    [InlineData("free product")]
    public void A_coupon_that_cannot_be_applied_is_not_entered(string problem)
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(CreateProduct(problem == "other author" ? Guid.NewGuid() : Author, problem == "free product" ? 0 : 20));
        var coupon = CreateCoupon(null, 10);
        var day = problem == "expired" ? coupon.ValidUntil.AddDays(1) : Today;
        HashSet<Guid> used = problem == "used" ? [coupon.Id] : [];

        var enter = () => CreatePurchasing().EnterCoupon(cart, coupon, used, day);

        enter.Should().Throw<DomainException>();
        cart.CouponIds.Should().BeEmpty();
    }

    [Fact]
    public void A_coupon_that_loses_to_a_better_one_on_its_product_is_still_entered()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = CreateProduct(Author, 20);
        cart.AddItem(product);
        var purchasing = CreatePurchasing();
        var better = CreateCoupon(product.Id, 50);
        purchasing.EnterCoupon(cart, better, new HashSet<Guid>(), Today);
        var worse = CreateCoupon(null, 10);

        purchasing.EnterCoupon(cart, worse, new HashSet<Guid>(), Today);

        cart.CouponIds.Should().Equal(better.Id, worse.Id);
    }

    [Fact]
    public void The_same_coupon_cannot_be_entered_twice()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(CreateProduct(Author, 20));
        var purchasing = CreatePurchasing();
        var coupon = CreateCoupon(null, 10);
        purchasing.EnterCoupon(cart, coupon, new HashSet<Guid>(), Today);

        var enter = () => purchasing.EnterCoupon(cart, coupon, new HashSet<Guid>(), Today);

        enter.Should().Throw<DomainException>();
        cart.CouponIds.Should().Equal(coupon.Id);
    }

    private static CartPurchasing CreatePurchasing()
    {
        return new CartPurchasing(new CouponAllocation());
    }

    private static Product CreateProduct(Guid authorId, int priceInEc)
    {
        return new Product(Guid.NewGuid(), "City walk", authorId, priceInEc);
    }

    private static Coupon CreateCoupon(Guid? productId, int discountPercent)
    {
        return new Coupon(
            Author, new CouponRequest(productId, discountPercent, Today.AddDays(10)), CouponCode.Generate(), CreatedAt);
    }
}
