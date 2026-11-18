using Payment.Domain.Products;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product CityWalk = new(CheckoutTourSeed.CityWalk.Id, CheckoutTourSeed.CityWalk.Name, WellKnownUsers.ThirdExplorer, 25);
    public static readonly Product FreeWalk = new(CheckoutTourSeed.FreeWalk.Id, CheckoutTourSeed.FreeWalk.Name, WellKnownUsers.ThirdExplorer, 0);
    public static readonly Product MuseumWalk = new(CheckoutTourSeed.MuseumWalk.Id, CheckoutTourSeed.MuseumWalk.Name, WellKnownUsers.ThirdExplorer, 15);
    public static readonly Product ArchivedWalk = new(CheckoutTourSeed.ArchivedWalk.Id, CheckoutTourSeed.ArchivedWalk.Name, WellKnownUsers.ThirdExplorer, 10);

    public static Product[] All => [CityWalk, FreeWalk, MuseumWalk, ArchivedWalk];
}
