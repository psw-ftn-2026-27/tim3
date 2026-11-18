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
        var createdOn = DateOnly.FromDateTime(createdAt);
        AuthorCityWalkCoupon = new Coupon(
            WellKnownUsers.ThirdExplorer, ProductSeed.CityWalk.Id, new CouponCode("CITY2099"), 10,
            new DateOnly(2099, 12, 31), createdOn, createdAt.AddDays(1));
        AuthorExpiredGeneralCoupon = new Coupon(
            WellKnownUsers.ThirdExplorer, null, new CouponCode("OLD20200"), 20,
            new DateOnly(2020, 1, 31), createdOn, createdAt);
        OtherAuthorRiverWalkCoupon = new Coupon(
            WellKnownUsers.FourthExplorer, ProductSeed.OtherAuthorRiverWalk.Id, new CouponCode("RIVER099"), 15,
            new DateOnly(2099, 12, 31), createdOn, createdAt);

        All = [AuthorCityWalkCoupon, AuthorExpiredGeneralCoupon, OtherAuthorRiverWalkCoupon];
    }
}
