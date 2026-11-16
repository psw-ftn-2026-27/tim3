using Payment.Domain.Products;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product CityWalk = new(Guid.NewGuid(), "City walk", WellKnownUsers.ThirdExplorer, 25);
    public static readonly Product FreeWalk = new(Guid.NewGuid(), "Free walk", WellKnownUsers.ThirdExplorer, 0);
    public static readonly Product MuseumWalk = new(Guid.NewGuid(), "Museum walk", WellKnownUsers.ThirdExplorer, 15);

    public static Product[] All => [CityWalk, FreeWalk, MuseumWalk];
}
