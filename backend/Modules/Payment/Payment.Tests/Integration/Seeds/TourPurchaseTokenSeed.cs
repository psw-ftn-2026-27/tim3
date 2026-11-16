using Payment.Domain.TourPurchases;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class TourPurchaseTokenSeed
{
    public static readonly TourPurchaseToken ExplorerMuseumWalk =
        new(WellKnownUsers.Explorer, ProductSeed.MuseumWalk.TourId, new DateTime(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken SecondExplorerCityWalk =
        new(WellKnownUsers.SecondExplorer, ProductSeed.CityWalk.TourId, new DateTime(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc));

    public static TourPurchaseToken[] All => [ExplorerMuseumWalk, SecondExplorerCityWalk];
}
