using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourPreferenceSeed
{
    public static readonly TourPreference ExplorersEasyHistoryWalks = new(WellKnownUsers.Explorer, [TourDifficulty.Easy, TourDifficulty.Moderate], TransportMode.Walking, ["istorija"], true);
    public static readonly TourPreference SecondExplorersCarTrips = new(WellKnownUsers.SecondExplorer, [], TransportMode.Car, [], false);

    public static TourPreference[] All => [ExplorersEasyHistoryWalks, SecondExplorersCarTrips];
}
