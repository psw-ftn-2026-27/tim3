using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Xunit;

namespace Payment.Tests.Unit.ShoppingCarts;

public class CouponAllocationTests
{
    private static readonly DateTime CreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(CreatedAt);
    private static readonly Guid Author = Guid.NewGuid();

    [Fact]
    public void A_general_coupon_falls_on_the_most_expensive_paid_product_of_its_author()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(CreateProduct(Author, 0));
        cart.AddItem(CreateProduct(Author, 10));
        var mostExpensive = CreateProduct(Author, 30);
        cart.AddItem(mostExpensive);
        cart.AddItem(CreateProduct(Guid.NewGuid(), 50));
        var coupon = CreateCoupon(null, 10);
        Enter(cart, coupon);

        var result = new CouponAllocation().Allocate(cart, [coupon], new HashSet<Guid>(), Today);

        result.Coupons.Should().ContainSingle().Which.TourId.Should().Be(mostExpensive.TourId);
    }

    [Fact]
    public void On_equal_price_a_general_coupon_falls_on_the_product_added_earlier()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var addedFirst = CreateProduct(Author, 20);
        cart.AddItem(addedFirst);
        cart.AddItem(CreateProduct(Author, 20));
        var coupon = CreateCoupon(null, 10);
        Enter(cart, coupon);

        var result = new CouponAllocation().Allocate(cart, [coupon], new HashSet<Guid>(), Today);

        result.Coupons.Should().ContainSingle().Which.TourId.Should().Be(addedFirst.TourId);
    }

    [Fact]
    public void A_product_coupon_falls_only_on_its_product()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = CreateProduct(Author, 10);
        cart.AddItem(product);
        cart.AddItem(CreateProduct(Author, 30));
        var coupon = CreateCoupon(product.Id, 10);
        Enter(cart, coupon);

        var result = new CouponAllocation().Allocate(cart, [coupon], new HashSet<Guid>(), Today);

        result.Coupons.Should().ContainSingle().Which.TourId.Should().Be(product.TourId);
    }

    [Fact]
    public void Two_coupons_on_two_products_are_both_applied()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var first = CreateProduct(Author, 20);
        var second = CreateProduct(Author, 40);
        cart.AddItem(first);
        cart.AddItem(second);
        var firstCoupon = CreateCoupon(first.Id, 10);
        var secondCoupon = CreateCoupon(second.Id, 50);
        Enter(cart, firstCoupon, secondCoupon);

        var result = new CouponAllocation().Allocate(cart, [firstCoupon, secondCoupon], new HashSet<Guid>(), Today);

        result.Coupons.Select(coupon => coupon.TourId).Should().Equal(first.TourId, second.TourId);
        result.TotalInEc.Should().Be(60 - 2 - 20);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Of_two_coupons_on_one_product_the_larger_percent_is_applied(bool largerEnteredFirst)
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = CreateProduct(Author, 40);
        cart.AddItem(product);
        var smaller = CreateCoupon(product.Id, 10);
        var larger = CreateCoupon(null, 25);
        Enter(cart, largerEnteredFirst ? [larger, smaller] : [smaller, larger]);

        var result = new CouponAllocation().Allocate(cart, [smaller, larger], new HashSet<Guid>(), Today);

        result.Coupons.Single(coupon => coupon.CouponId == larger.Id).TourId.Should().Be(product.TourId);
        result.Coupons.Single(coupon => coupon.CouponId == smaller.Id).Reason
            .Should().Be(CouponNotAppliedReason.BetterCouponApplied);
        result.TotalInEc.Should().Be(30);
    }

    [Fact]
    public void On_equal_percent_the_coupon_entered_first_is_applied()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = CreateProduct(Author, 40);
        cart.AddItem(product);
        var enteredFirst = CreateCoupon(null, 10);
        var enteredSecond = CreateCoupon(product.Id, 10);
        Enter(cart, enteredFirst, enteredSecond);

        var result = new CouponAllocation().Allocate(cart, [enteredSecond, enteredFirst], new HashSet<Guid>(), Today);

        result.Coupons.Select(coupon => coupon.Reason)
            .Should().Equal(null, CouponNotAppliedReason.BetterCouponApplied);
    }

    [Fact]
    public void A_general_coupon_that_loses_does_not_move_to_another_product()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var mostExpensive = CreateProduct(Author, 40);
        cart.AddItem(mostExpensive);
        cart.AddItem(CreateProduct(Author, 20));
        var productCoupon = CreateCoupon(mostExpensive.Id, 50);
        var generalCoupon = CreateCoupon(null, 10);
        Enter(cart, productCoupon, generalCoupon);

        var result = new CouponAllocation().Allocate(cart, [productCoupon, generalCoupon], new HashSet<Guid>(), Today);

        result.Coupons.Single(coupon => coupon.CouponId == generalCoupon.Id)
            .Should().Be(new AllocatedCoupon(generalCoupon.Id, null, 0, CouponNotAppliedReason.BetterCouponApplied));
        result.TotalInEc.Should().Be(60 - 20);
    }

    [Theory]
    [InlineData(true, CouponNotAppliedReason.Expired)]
    [InlineData(false, CouponNotAppliedReason.AlreadyUsed)]
    public void An_expired_or_already_used_coupon_is_not_applied(bool expired, CouponNotAppliedReason reason)
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(CreateProduct(Author, 40));
        var coupon = CreateCoupon(null, 10);
        Enter(cart, coupon);
        var day = expired ? coupon.ValidUntil.AddDays(1) : Today;
        HashSet<Guid> used = expired ? [] : [coupon.Id];

        var result = new CouponAllocation().Allocate(cart, [coupon], used, day);

        result.Coupons.Should().ContainSingle().Which.Should().Be(new AllocatedCoupon(coupon.Id, null, 0, reason));
        result.TotalInEc.Should().Be(40);
    }

    [Fact]
    public void The_discount_is_taken_from_the_price_in_the_cart_and_rounded_down()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = CreateProduct(Author, 25);
        cart.AddItem(product);
        var coupon = CreateCoupon(null, 10);
        Enter(cart, coupon);

        var result = new CouponAllocation().Allocate(cart, [coupon], new HashSet<Guid>(), Today);

        result.Coupons.Should().ContainSingle().Which.DiscountInEc.Should().Be(2);
        result.TotalInEc.Should().Be(23);
    }

    [Fact]
    public void A_discount_that_rounds_down_to_zero_is_not_applied()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.AddItem(CreateProduct(Author, 5));
        var coupon = CreateCoupon(null, 10);
        Enter(cart, coupon);

        var result = new CouponAllocation().Allocate(cart, [coupon], new HashSet<Guid>(), Today);

        result.Coupons.Should().ContainSingle()
            .Which.Should().Be(new AllocatedCoupon(coupon.Id, null, 0, CouponNotAppliedReason.DiscountTooSmall));
        result.TotalInEc.Should().Be(5);
    }

    private static void Enter(ShoppingCart cart, params Coupon[] coupons)
    {
        var purchasing = new CartPurchasing(new CartPricing(new CouponAllocation()));
        foreach (var coupon in coupons)
        {
            purchasing.EnterCoupon(cart, coupon, new HashSet<Guid>(), Today);
        }
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
