using Exploration.Domain.Tours;
using Shared.Tests;

namespace Chats.Tests.Integration.Seeds;

// A tour of Exploration with a known author, so a tour chat has a privileged user (P1).
// Chats reads the author only through IExplorationApi; this seed only puts the tour in the database.
internal static class TourSeed
{
    public static readonly Tour FortressWalkByExplorer = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave i završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija"], new GroupSize(2, 10));

    public static Tour[] All => [FortressWalkByExplorer];
}
