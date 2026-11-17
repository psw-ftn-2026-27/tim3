using Maps.Core.UserPositions;
using Shared.Tests;

namespace Maps.Tests.Integration.Seeds;

internal static class UserPositionSeed
{
    public static readonly UserPosition ExplorerAtFreedomSquare;

    public static readonly UserPosition[] All;

    static UserPositionSeed()
    {
        ExplorerAtFreedomSquare = new UserPosition(WellKnownUsers.Explorer,
            new Coordinates(45.2551, 19.8451), new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc));

        All = [ExplorerAtFreedomSquare];
    }
}
