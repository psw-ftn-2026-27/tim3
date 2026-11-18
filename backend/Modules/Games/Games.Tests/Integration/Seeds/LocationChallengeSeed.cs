using Games.Domain.LocationChallenges;
using Games.Domain.Locations;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeSeed
{
    public static readonly LocationChallenge SecretsOfTheFortress = new("Tajne tvrđave", new Location(45.2517, 19.8634), 300,
        [new HiddenPoint("/api/media/images/sat-na-tvrdjavi.jpg", new Location(45.2524, 19.8619)),
         new HiddenPoint("/api/media/images/kapija.jpg", new Location(45.2509, 19.8641))]);
    public static readonly LocationChallenge CornersOfTheOldTown = new("Kutci starog grada", new Location(45.2551, 19.8451), 150,
        [new HiddenPoint("/api/media/images/katedrala.jpg", new Location(45.2554, 19.8450))]);

    public static LocationChallenge[] All => [SecretsOfTheFortress, CornersOfTheOldTown];
}
