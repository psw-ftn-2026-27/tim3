using Exploration.Domain.Tours;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class CheckoutTourSeed
{
    public static readonly Tour CityWalk;
    public static readonly Tour FreeWalk;
    public static readonly Tour MuseumWalk;
    public static readonly Tour ArchivedWalk;
    public static readonly Tour PublishableWalk;

    static CheckoutTourSeed()
    {
        const string description =
            "The walk goes through the city centre, across the main square and along the river before ending in the park.";
        var author = WellKnownUsers.ThirdExplorer;
        CityWalk = new Tour(author, "City walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        CityWalk.AddTransportTime(TransportMode.Walking, 60);
        CityWalk.Publish(TourPrice.Paid(25));

        FreeWalk = new Tour(author, "Free walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        FreeWalk.AddTransportTime(TransportMode.Walking, 60);
        FreeWalk.Publish(TourPrice.Free());

        MuseumWalk = new Tour(author, "Museum walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        MuseumWalk.AddTransportTime(TransportMode.Walking, 60);
        MuseumWalk.Publish(TourPrice.Paid(15));

        ArchivedWalk = new Tour(author, "Archived walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        ArchivedWalk.AddTransportTime(TransportMode.Walking, 60);
        ArchivedWalk.Publish(TourPrice.Paid(10));
        ArchivedWalk.Archive();

        PublishableWalk = new Tour(author, "Publishable walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        PublishableWalk.AddTransportTime(TransportMode.Walking, 60);
    }

    public static Tour[] All => [CityWalk, FreeWalk, MuseumWalk, ArchivedWalk, PublishableWalk];
}
