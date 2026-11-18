using Payment.Domain.TourPurchases;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class TourPurchaseTokenSeed
{
    public static readonly TourPurchaseToken ExplorerMuseumWalk =
        new(WellKnownUsers.Explorer, ProductSeed.MuseumWalk.TourId, new DateTime(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken SecondExplorerCityWalk =
        new(WellKnownUsers.SecondExplorer, ProductSeed.CityWalk.TourId, new DateTime(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc));

    // The general coupon would fall on City walk in this buyer's cart, but it was already used for Museum walk.
    public static readonly TourPurchaseToken CouponUserMuseumWalkWithGeneralCoupon =
        new(WellKnownUsers.SixthExplorer, ProductSeed.MuseumWalk.TourId, new DateTime(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc),
            CouponSeed.AuthorGeneralCoupon.Id, 3);

    public static TourPurchaseToken[] All =>
        [ExplorerMuseumWalk, SecondExplorerCityWalk, CouponUserMuseumWalkWithGeneralCoupon];
}
