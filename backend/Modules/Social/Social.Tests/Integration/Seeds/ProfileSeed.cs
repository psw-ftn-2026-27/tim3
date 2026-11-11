using Shared.Tests;
using Social.Domain.Profiles;

namespace Social.Tests.Integration.Seeds;

// SecondExplorer deliberately has no profile, so tests can show a recipient without a
// profile and requests to SecondExplorer that live in their senders' profiles.
// ThirdExplorer and FourthExplorer follow each other, and each also follows a user who does
// not follow back, so ThirdExplorer has one follower and two followed users.
internal static class ProfileSeed
{
    public static readonly Profile ExplorerPendingToSecondExplorer;
    public static readonly Profile AdministratorRejectedBySecondExplorerPendingToExplorer;
    public static readonly Profile ThirdExplorerFollowingFourthExplorerAndAnotherUser;
    public static readonly Profile FourthExplorerFollowingThirdExplorerAndAUserWhoDoesNotFollowBack;
    public static readonly Profile FollowingAUserWithoutAProfile;

    static ProfileSeed()
    {
        ExplorerPendingToSecondExplorer = new(WellKnownUsers.Explorer);
        ExplorerPendingToSecondExplorer.SendFollowRequest(WellKnownUsers.SecondExplorer, false, DateTime.UtcNow);

        AdministratorRejectedBySecondExplorerPendingToExplorer = new(WellKnownUsers.Administrator);
        AdministratorRejectedBySecondExplorerPendingToExplorer.SendFollowRequest(WellKnownUsers.SecondExplorer, false, DateTime.UtcNow);
        AdministratorRejectedBySecondExplorerPendingToExplorer.RejectFollowRequest(
            AdministratorRejectedBySecondExplorerPendingToExplorer.SentRequests[0].Id, WellKnownUsers.SecondExplorer, DateTime.UtcNow);
        AdministratorRejectedBySecondExplorerPendingToExplorer.SendFollowRequest(WellKnownUsers.Explorer, false, DateTime.UtcNow);

        ThirdExplorerFollowingFourthExplorerAndAnotherUser = new(WellKnownUsers.ThirdExplorer);
        ThirdExplorerFollowingFourthExplorerAndAnotherUser.SendFollowRequest(WellKnownUsers.FourthExplorer, true, DateTime.UtcNow);
        ThirdExplorerFollowingFourthExplorerAndAnotherUser.SendFollowRequest(Guid.NewGuid(), true, DateTime.UtcNow);

        FourthExplorerFollowingThirdExplorerAndAUserWhoDoesNotFollowBack = new(WellKnownUsers.FourthExplorer);
        FourthExplorerFollowingThirdExplorerAndAUserWhoDoesNotFollowBack.SendFollowRequest(WellKnownUsers.ThirdExplorer, true, DateTime.UtcNow);
        FourthExplorerFollowingThirdExplorerAndAUserWhoDoesNotFollowBack.SendFollowRequest(Guid.NewGuid(), true, DateTime.UtcNow);

        FollowingAUserWithoutAProfile = new(Guid.NewGuid());
        FollowingAUserWithoutAProfile.SendFollowRequest(Guid.NewGuid(), true, DateTime.UtcNow);
    }

    public static Profile[] All =>
    [
        ExplorerPendingToSecondExplorer,
        AdministratorRejectedBySecondExplorerPendingToExplorer,
        ThirdExplorerFollowingFourthExplorerAndAnotherUser,
        FourthExplorerFollowingThirdExplorerAndAUserWhoDoesNotFollowBack,
        FollowingAUserWithoutAProfile
    ];
}
