using Games.Domain.Locations;
using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge SunsetOnTheFortress = new("Zalazak na tvrđavi", new ActivePeriod(new TimeOnly(18, 0), new TimeOnly(20, 30)), 5, 10, 300, "Bravo, zajedno ste dočekali zalazak sunca!", new Location(45.2517, 19.8616));
    public static readonly SocialChallenge MidnightOnTheBridge = new("Ponoć na mostu", new ActivePeriod(new TimeOnly(22, 0), new TimeOnly(2, 0)), 2, 3, 0, "Hvala na druženju posle ponoći!", new Location(45.2560, 19.8580));
    public static readonly SocialChallenge DawnOnTheBeachWithoutLocation = new("Svitanje na Štrandu", new ActivePeriod(new TimeOnly(5, 0), new TimeOnly(6, 30)), 3, 5, 100, "Dobro jutro sa Štranda!", null);
    public static readonly SocialChallenge PublishedNoonOnTheSquare;
    public static readonly SocialChallenge RetiredMorningOnTheQuay;
    public static readonly SocialChallenge ArchivedPicnicInThePark;

    static SocialChallengeSeed()
    {
        PublishedNoonOnTheSquare = new("Podne na trgu", new ActivePeriod(new TimeOnly(12, 0), new TimeOnly(13, 0)), 6, 10, 200, "Podne je vaše!", new Location(45.2551, 19.8452));
        PublishedNoonOnTheSquare.Publish();

        RetiredMorningOnTheQuay = new("Jutro na keju", new ActivePeriod(new TimeOnly(7, 0), new TimeOnly(9, 0)), 4, 5, 150, "Dobro jutro, ekipo!", new Location(45.2522, 19.8622));
        RetiredMorningOnTheQuay.Publish();
        RetiredMorningOnTheQuay.Retire();

        ArchivedPicnicInThePark = new("Piknik u Dunavskom parku", new ActivePeriod(new TimeOnly(15, 0), new TimeOnly(18, 0)), 8, 12, 400, "Hvala na pikniku!", new Location(45.2548, 19.8512));
        ArchivedPicnicInThePark.Publish();
        ArchivedPicnicInThePark.Archive();
    }

    public static SocialChallenge[] All => [SunsetOnTheFortress, MidnightOnTheBridge, DawnOnTheBeachWithoutLocation, PublishedNoonOnTheSquare, RetiredMorningOnTheQuay, ArchivedPicnicInThePark];
}
