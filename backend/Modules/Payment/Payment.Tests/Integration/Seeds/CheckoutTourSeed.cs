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
    public static readonly Tour OtherAuthorRiverWalk;

    static CheckoutTourSeed()
    {
        const string description =
            "The walk goes through the city centre, across the main square and along the river before ending in the park.";
        var author = WellKnownUsers.ThirdExplorer;
        CityWalk = new Tour(author, "City walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        CityWalk.AddTransportTime(TransportMode.Walking, 60);
        CityWalk.DefineKeyPoints(
        [
            new KeyPoint("Start", "Meeting point", "/api/media/images/start.png", new GeoLocation(45.24, 19.85), null, true),
            new KeyPoint("Finish", "End of the walk", "/api/media/images/finish.png", new GeoLocation(45.25, 19.86), null, false)
        ], new TourLength(2, false));
        CityWalk.Publish(TourPrice.Paid(25));

        FreeWalk = new Tour(author, "Free walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        FreeWalk.AddTransportTime(TransportMode.Walking, 60);
        FreeWalk.DefineKeyPoints(
        [
            new KeyPoint("Start", "Meeting point", "/api/media/images/start.png", new GeoLocation(45.24, 19.85), null, true),
            new KeyPoint("Finish", "End of the walk", "/api/media/images/finish.png", new GeoLocation(45.25, 19.86), null, false)
        ], new TourLength(2, false));
        FreeWalk.Publish(TourPrice.Free());

        MuseumWalk = new Tour(author, "Museum walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        MuseumWalk.AddTransportTime(TransportMode.Walking, 60);
        MuseumWalk.DefineKeyPoints(
        [
            new KeyPoint("Start", "Meeting point", "/api/media/images/start.png", new GeoLocation(45.24, 19.85), null, true),
            new KeyPoint("Finish", "End of the walk", "/api/media/images/finish.png", new GeoLocation(45.25, 19.86), null, false)
        ], new TourLength(2, false));
        MuseumWalk.Publish(TourPrice.Paid(15));

        ArchivedWalk = new Tour(author, "Archived walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        ArchivedWalk.AddTransportTime(TransportMode.Walking, 60);
        ArchivedWalk.DefineKeyPoints(
        [
            new KeyPoint("Start", "Meeting point", "/api/media/images/start.png", new GeoLocation(45.24, 19.85), null, true),
            new KeyPoint("Finish", "End of the walk", "/api/media/images/finish.png", new GeoLocation(45.25, 19.86), null, false)
        ], new TourLength(2, false));
        ArchivedWalk.Publish(TourPrice.Paid(10));
        ArchivedWalk.Archive();

        PublishableWalk = new Tour(author, "Publishable walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        PublishableWalk.AddTransportTime(TransportMode.Walking, 60);
        PublishableWalk.DefineKeyPoints(
        [
            new KeyPoint("Start", "Meeting point", "/api/media/images/start.png", new GeoLocation(45.24, 19.85), null, true),
            new KeyPoint("Finish", "End of the walk", "/api/media/images/finish.png", new GeoLocation(45.25, 19.86), null, false)
        ], new TourLength(2, false));

        OtherAuthorRiverWalk = new Tour(
            WellKnownUsers.FourthExplorer, "River walk", description, TourDifficulty.Easy, ["walking"], new GroupSize(2, 10));
        OtherAuthorRiverWalk.AddTransportTime(TransportMode.Walking, 60);
        OtherAuthorRiverWalk.DefineKeyPoints(
        [
            new KeyPoint("Start", "Meeting point", "/api/media/images/start.png", new GeoLocation(45.24, 19.85), null, true),
            new KeyPoint("Finish", "End of the walk", "/api/media/images/finish.png", new GeoLocation(45.25, 19.86), null, false)
        ], new TourLength(2, false));
        OtherAuthorRiverWalk.Publish(TourPrice.Paid(20));
    }

    public static Tour[] All => [CityWalk, FreeWalk, MuseumWalk, ArchivedWalk, PublishableWalk, OtherAuthorRiverWalk];
}
