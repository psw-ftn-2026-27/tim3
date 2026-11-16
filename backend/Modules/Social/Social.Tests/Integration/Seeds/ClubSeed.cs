using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

// Hikers and FortressLovers have only their owners as members. In Cyclists, owned by Explorer,
// FourthExplorer is a member and ThirdExplorer's request to join is pending. Runners, owned by
// Explorer, has a pending invitation for ThirdExplorer; Photographers, owned by SecondExplorer,
// has a pending invitation for FourthExplorer.
internal static class ClubSeed
{
    public static readonly Club Hikers;
    public static readonly Club FortressLovers;
    public static readonly Club CyclistsWithAMemberAndAPendingRequest;
    public static readonly Club RunnersWithAPendingInvitation;
    public static readonly Club PhotographersWithAPendingInvitation;

    static ClubSeed()
    {
        Hikers = new(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarskih tura po Fruškoj gori.", ["/api/media/images/planinari.jpg"]);

        FortressLovers = new(WellKnownUsers.SecondExplorer, "Ljubitelji tvrđava", "Klub za istraživače starih tvrđava i podzemnih galerija.", ["/api/media/images/tvrdjava.jpg", "/api/media/images/galerija.jpg"]);

        CyclistsWithAMemberAndAPendingRequest = new(WellKnownUsers.Explorer, "Biciklisti Podunavlja", "Klub ljubitelja biciklističkih tura uz Dunav.", ["/api/media/images/biciklisti.jpg"]);
        CyclistsWithAMemberAndAPendingRequest.RequestToJoin(WellKnownUsers.FourthExplorer);
        CyclistsWithAMemberAndAPendingRequest.AcceptRequest(WellKnownUsers.Explorer, WellKnownUsers.FourthExplorer);
        CyclistsWithAMemberAndAPendingRequest.RequestToJoin(WellKnownUsers.ThirdExplorer);

        RunnersWithAPendingInvitation = new(WellKnownUsers.Explorer, "Trkači Novog Sada", "Klub ljubitelja trčanja kroz parkove i uz kej.", ["/api/media/images/trkaci.jpg"]);
        RunnersWithAPendingInvitation.Invite(WellKnownUsers.Explorer, WellKnownUsers.ThirdExplorer);

        PhotographersWithAPendingInvitation = new(WellKnownUsers.SecondExplorer, "Fotografi prirode", "Klub za istraživače koji fotografišu prirodu na turama.", ["/api/media/images/fotografi.jpg"]);
        PhotographersWithAPendingInvitation.Invite(WellKnownUsers.SecondExplorer, WellKnownUsers.FourthExplorer);
    }

    public static Club[] All =>
    [
        Hikers,
        FortressLovers,
        CyclistsWithAMemberAndAPendingRequest,
        RunnersWithAPendingInvitation,
        PhotographersWithAPendingInvitation
    ];
}
