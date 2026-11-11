using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club Hikers = new(WellKnownUsers.Explorer, "Planinari", "Klub ljubitelja planinarskih tura po Fruškoj gori.", ["/api/media/images/planinari.jpg"]);
    public static readonly Club FortressLovers = new(WellKnownUsers.SecondExplorer, "Ljubitelji tvrđava", "Klub za istraživače starih tvrđava i podzemnih galerija.", ["/api/media/images/tvrdjava.jpg", "/api/media/images/galerija.jpg"]);

    public static Club[] All => [Hikers, FortressLovers];
}
