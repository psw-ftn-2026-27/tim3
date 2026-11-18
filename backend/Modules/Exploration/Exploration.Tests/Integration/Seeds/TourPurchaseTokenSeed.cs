using Payment.Domain.TourPurchases;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourPurchaseTokenSeed
{
    public static readonly TourPurchaseToken ArchivedFreeTourForSecondExplorer = new(
        WellKnownUsers.SecondExplorer, ProductSeed.ArchivedHomesteads.TourId,
        new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken PublishedOldTownForThirdExplorer = new(
        WellKnownUsers.ThirdExplorer, TourSeed.PublishedOldTownWithHiddenMiddleKeyPoint.Id,
        new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc));

    public static readonly TourPurchaseToken MissingTourForSecondExplorer = new(
        WellKnownUsers.SecondExplorer, Guid.Parse("90000000-0000-0000-0000-000000000021"),
        new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc));

    public static TourPurchaseToken[] All =>
        [ArchivedFreeTourForSecondExplorer, PublishedOldTownForThirdExplorer, MissingTourForSecondExplorer];
}
