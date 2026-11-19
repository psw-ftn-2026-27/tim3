using Maps.Core.UserPositions;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class UserPositionSeed
{
    public static readonly UserPosition ExplorerAtTheUndiscoveredGateOfTheFortress;
    public static readonly UserPosition SecondExplorerNearTheStartOfTheOldTown;
    public static readonly UserPosition ThirdExplorerAtTheRailwayStation;

    public static readonly UserPosition[] All;

    static UserPositionSeed()
    {
        ExplorerAtTheUndiscoveredGateOfTheFortress = new UserPosition(WellKnownUsers.Explorer,
            new Coordinates(45.25092, 19.8641), new DateTime(2026, 11, 15, 10, 30, 0, DateTimeKind.Utc));

        SecondExplorerNearTheStartOfTheOldTown = new UserPosition(WellKnownUsers.SecondExplorer,
            new Coordinates(45.2552, 19.8451), new DateTime(2026, 11, 15, 11, 0, 0, DateTimeKind.Utc));

        ThirdExplorerAtTheRailwayStation = new UserPosition(WellKnownUsers.ThirdExplorer,
            new Coordinates(45.2671, 19.8335), new DateTime(2026, 11, 15, 11, 30, 0, DateTimeKind.Utc));

        All = [ExplorerAtTheUndiscoveredGateOfTheFortress, SecondExplorerNearTheStartOfTheOldTown, ThirdExplorerAtTheRailwayStation];
    }
}
