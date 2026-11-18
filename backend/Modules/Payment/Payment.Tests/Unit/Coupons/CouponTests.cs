using FluentAssertions;
using Payment.Domain.Coupons;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Unit.Coupons;

public class CouponTests
{
    private static readonly DateTime CreatedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(CreatedAt);

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void A_coupon_with_a_discount_at_the_limits_is_accepted(int discountPercent)
    {
        var coupon = CreateCoupon(discountPercent, Today.AddDays(10));

        coupon.DiscountPercent.Should().Be(discountPercent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void A_coupon_with_a_discount_outside_one_to_one_hundred_percent_is_rejected(int discountPercent)
    {
        var create = () => CreateCoupon(discountPercent, Today.AddDays(10));

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_coupon_valid_until_a_day_before_today_is_rejected()
    {
        var create = () => CreateCoupon(10, Today.AddDays(-1));

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_coupon_valid_until_today_is_accepted()
    {
        var coupon = CreateCoupon(10, Today);

        coupon.ValidUntil.Should().Be(Today);
    }

    [Fact]
    public void A_coupon_is_not_expired_on_its_last_valid_day()
    {
        var coupon = CreateCoupon(10, Today.AddDays(10));

        var isExpired = coupon.IsExpiredOn(Today.AddDays(10));

        isExpired.Should().BeFalse();
    }

    [Fact]
    public void A_coupon_is_expired_the_day_after_its_last_valid_day()
    {
        var coupon = CreateCoupon(10, Today.AddDays(10));

        var isExpired = coupon.IsExpiredOn(Today.AddDays(11));

        isExpired.Should().BeTrue();
    }

    private static Coupon CreateCoupon(int discountPercent, DateOnly validUntil)
    {
        return new Coupon(
            WellKnownUsers.ThirdExplorer, null, new CouponCode("ABCD1234"), discountPercent, validUntil, Today, CreatedAt);
    }
}
