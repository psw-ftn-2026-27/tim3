using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

// Hikers and FortressLovers have only their owners as members. In Cyclists, owned by Explorer,
// FourthExplorer is a member and ThirdExplorer's request to join is pending.
internal static class ClubSeed
{
    public static readonly Club Hikers;
    public static readonly Club FortressLovers;
    public static readonly Club CyclistsWithAMemberAndAPendingRequest;

    static ClubSeed()
    {
        Hikers = new(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarskih tura po Fruškoj gori.", ["/api/media/images/planinari.jpg"]);

        FortressLovers = new(WellKnownUsers.SecondExplorer, "Ljubitelji tvrđava", "Klub za istraživače starih tvrđava i podzemnih galerija.", ["/api/media/images/tvrdjava.jpg", "/api/media/images/galerija.jpg"]);

        CyclistsWithAMemberAndAPendingRequest = new(WellKnownUsers.Explorer, "Biciklisti Podunavlja", "Klub ljubitelja biciklističkih tura uz Dunav.", ["/api/media/images/biciklisti.jpg"]);
        CyclistsWithAMemberAndAPendingRequest.RequestToJoin(WellKnownUsers.FourthExplorer);
        CyclistsWithAMemberAndAPendingRequest.AcceptRequest(WellKnownUsers.Explorer, WellKnownUsers.FourthExplorer);
        CyclistsWithAMemberAndAPendingRequest.RequestToJoin(WellKnownUsers.ThirdExplorer);
    }

    public static Club[] All => [Hikers, FortressLovers, CyclistsWithAMemberAndAPendingRequest];
}
