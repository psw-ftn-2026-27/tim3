using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeParticipationSeed
{
    public static readonly LocationChallengeParticipation ExplorerHalfwayThroughTheFortress;
    public static readonly LocationChallengeParticipation ExplorerSolvedTheOldTown;
    public static readonly LocationChallengeParticipation FourthExplorerStartedTheFortress;

    static LocationChallengeParticipationSeed()
    {
        var fortress = LocationChallengeSeed.SecretsOfTheFortress;
        ExplorerHalfwayThroughTheFortress = new(WellKnownUsers.Explorer, fortress.Id, new SearchCircle(fortress.StartLocation, 180));
        ExplorerHalfwayThroughTheFortress.Discover(fortress.HiddenPoints[0].Id, fortress.HiddenPoints.Count, fortress.ExperienceGained, new DateTime(2026, 11, 15, 10, 0, 0, DateTimeKind.Utc));

        var oldTown = LocationChallengeSeed.CornersOfTheOldTown;
        ExplorerSolvedTheOldTown = new(WellKnownUsers.Explorer, oldTown.Id, new SearchCircle(oldTown.StartLocation, 90));
        ExplorerSolvedTheOldTown.Discover(oldTown.HiddenPoints[0].Id, oldTown.HiddenPoints.Count, oldTown.ExperienceGained, new DateTime(2026, 11, 14, 16, 30, 0, DateTimeKind.Utc));

        FourthExplorerStartedTheFortress = new(WellKnownUsers.FourthExplorer, fortress.Id, new SearchCircle(new Location(fortress.StartLocation.Latitude, fortress.StartLocation.Longitude), 180));
    }

    public static LocationChallengeParticipation[] All => [ExplorerHalfwayThroughTheFortress, ExplorerSolvedTheOldTown, FourthExplorerStartedTheFortress];
}
