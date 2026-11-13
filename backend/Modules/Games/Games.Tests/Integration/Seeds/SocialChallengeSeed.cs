using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge SunsetOnTheFortress = new("Zalazak na tvrđavi", new ActivePeriod(new TimeOnly(18, 0), new TimeOnly(20, 30)), 5, 10, 300, "Bravo, zajedno ste dočekali zalazak sunca!");
    public static readonly SocialChallenge MidnightOnTheBridge = new("Ponoć na mostu", new ActivePeriod(new TimeOnly(22, 0), new TimeOnly(2, 0)), 2, 3, 0, "Hvala na druženju posle ponoći!");

    public static SocialChallenge[] All => [SunsetOnTheFortress, MidnightOnTheBridge];
}
