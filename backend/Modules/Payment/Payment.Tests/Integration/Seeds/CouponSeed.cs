using Payment.Domain.Coupons;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class CouponSeed
{
    public static readonly Coupon AuthorCityWalkCoupon;
    public static readonly Coupon AuthorExpiredGeneralCoupon;
    public static readonly Coupon OtherAuthorRiverWalkCoupon;

    public static readonly Coupon[] All;

    static CouponSeed()
    {
        var createdAt = new DateTime(2020, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        AuthorCityWalkCoupon = new Coupon(
            WellKnownUsers.ThirdExplorer, new CouponRequest(ProductSeed.CityWalk.Id, 10, new DateOnly(2099, 12, 31)),
            new CouponCode("CITY2099"), createdAt.AddDays(1));
        AuthorExpiredGeneralCoupon = new Coupon(
            WellKnownUsers.ThirdExplorer, new CouponRequest(null, 20, new DateOnly(2020, 1, 31)),
            new CouponCode("OLD20200"), createdAt);
        OtherAuthorRiverWalkCoupon = new Coupon(
            WellKnownUsers.FourthExplorer,
            new CouponRequest(ProductSeed.OtherAuthorRiverWalk.Id, 15, new DateOnly(2099, 12, 31)),
            new CouponCode("RIVER099"), createdAt);

        All = [AuthorCityWalkCoupon, AuthorExpiredGeneralCoupon, OtherAuthorRiverWalkCoupon];
    }
}
