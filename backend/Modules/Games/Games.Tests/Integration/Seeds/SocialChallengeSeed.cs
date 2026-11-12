using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge SunsetOnTheFortress = new("Zalazak na tvrđavi", new ActivePeriod(new TimeOnly(18, 0), new TimeOnly(20, 30)), 5, 10, 300, "Bravo, zajedno ste dočekali zalazak sunca!");
    public static readonly SocialChallenge MidnightOnTheBridge = new("Ponoć na mostu", new ActivePeriod(new TimeOnly(22, 0), new TimeOnly(2, 0)), 2, 3, 0, "Hvala na druženju posle ponoći!");
    public static readonly SocialChallenge PublishedNoonOnTheSquare;
    public static readonly SocialChallenge RetiredMorningOnTheQuay;
    public static readonly SocialChallenge ArchivedPicnicInThePark;

    static SocialChallengeSeed()
    {
        PublishedNoonOnTheSquare = new("Podne na trgu", new ActivePeriod(new TimeOnly(12, 0), new TimeOnly(13, 0)), 6, 10, 200, "Podne je vaše!");
        PublishedNoonOnTheSquare.Publish();

        RetiredMorningOnTheQuay = new("Jutro na keju", new ActivePeriod(new TimeOnly(7, 0), new TimeOnly(9, 0)), 4, 5, 150, "Dobro jutro, ekipo!");
        RetiredMorningOnTheQuay.Publish();
        RetiredMorningOnTheQuay.Retire();

        ArchivedPicnicInThePark = new("Piknik u Dunavskom parku", new ActivePeriod(new TimeOnly(15, 0), new TimeOnly(18, 0)), 8, 12, 400, "Hvala na pikniku!");
        ArchivedPicnicInThePark.Publish();
        ArchivedPicnicInThePark.Archive();
    }

    public static SocialChallenge[] All => [SunsetOnTheFortress, MidnightOnTheBridge, PublishedNoonOnTheSquare, RetiredMorningOnTheQuay, ArchivedPicnicInThePark];
}
