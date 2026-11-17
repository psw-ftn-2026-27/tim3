using Payment.Domain.Products;

namespace Exploration.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product SavedDuringPublication = new(
        TourSeed.PublicationWithSavedOffer.Id, TourSeed.PublicationWithSavedOffer.Name, TourSeed.PublicationWithSavedOffer.AuthorId, 35);

    public static readonly Product PublishedVineyards = new(
        TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.Name, TourSeed.PublishedVineyards.AuthorId, 75);

    public static readonly Product ArchivedHomesteads = new(
        TourSeed.ArchivedHomesteads.Id, TourSeed.ArchivedHomesteads.Name, TourSeed.ArchivedHomesteads.AuthorId, 0);

    public static Product[] All => [SavedDuringPublication, PublishedVineyards, ArchivedHomesteads];
}
