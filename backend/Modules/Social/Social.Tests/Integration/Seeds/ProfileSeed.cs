using Shared.Tests;
using Social.Domain.Profiles;

namespace Social.Tests.Integration.Seeds;

// SecondExplorer deliberately has no profile, so tests can show a recipient without a
// profile and requests to SecondExplorer that live in their senders' profiles.
internal static class ProfileSeed
{
    public static readonly Profile ExplorerPendingToSecondExplorer;
    public static readonly Profile AdministratorRejectedBySecondExplorerPendingToExplorer;

    static ProfileSeed()
    {
        ExplorerPendingToSecondExplorer = new(WellKnownUsers.Explorer);
        ExplorerPendingToSecondExplorer.SendFollowRequest(WellKnownUsers.SecondExplorer, false, DateTime.UtcNow);

        AdministratorRejectedBySecondExplorerPendingToExplorer = new(WellKnownUsers.Administrator);
        AdministratorRejectedBySecondExplorerPendingToExplorer.SendFollowRequest(WellKnownUsers.SecondExplorer, false, DateTime.UtcNow);
        AdministratorRejectedBySecondExplorerPendingToExplorer.RejectFollowRequest(
            AdministratorRejectedBySecondExplorerPendingToExplorer.SentRequests[0].Id, WellKnownUsers.SecondExplorer, DateTime.UtcNow);
        AdministratorRejectedBySecondExplorerPendingToExplorer.SendFollowRequest(WellKnownUsers.Explorer, false, DateTime.UtcNow);
    }

    public static Profile[] All => [ExplorerPendingToSecondExplorer, AdministratorRejectedBySecondExplorerPendingToExplorer];
}
