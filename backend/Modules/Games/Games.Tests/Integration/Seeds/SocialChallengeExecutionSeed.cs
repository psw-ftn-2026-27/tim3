using Games.Domain.SocialChallengeExecutions;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeExecutionSeed
{
    private static readonly DateTime StartedAt = new(2026, 11, 20, 11, 10, 0, DateTimeKind.Utc);

    public static readonly SocialChallengeExecution NoonOnTheSquareSolvedByExplorerAndSecondExplorer;
    public static readonly SocialChallengeExecution NoonOnTheSquareSolvedBySecondAndThirdExplorer;

    static SocialChallengeExecutionSeed()
    {
        NoonOnTheSquareSolvedByExplorerAndSecondExplorer = new(SocialChallengeSeed.PublishedNoonOnTheSquare.Id, StartedAt);
        NoonOnTheSquareSolvedByExplorerAndSecondExplorer.Join(WellKnownUsers.Explorer, StartedAt);
        NoonOnTheSquareSolvedByExplorerAndSecondExplorer.Join(WellKnownUsers.SecondExplorer, StartedAt);
        NoonOnTheSquareSolvedByExplorerAndSecondExplorer.Solve(StartedAt.AddSeconds(10), SocialChallengeSeed.PublishedNoonOnTheSquare.ExperienceGained, []);

        NoonOnTheSquareSolvedBySecondAndThirdExplorer = new(SocialChallengeSeed.PublishedNoonOnTheSquare.Id, StartedAt.AddDays(1));
        NoonOnTheSquareSolvedBySecondAndThirdExplorer.Join(WellKnownUsers.SecondExplorer, StartedAt.AddDays(1));
        NoonOnTheSquareSolvedBySecondAndThirdExplorer.Join(WellKnownUsers.ThirdExplorer, StartedAt.AddDays(1));
        NoonOnTheSquareSolvedBySecondAndThirdExplorer.Solve(StartedAt.AddDays(1).AddSeconds(10), SocialChallengeSeed.PublishedNoonOnTheSquare.ExperienceGained, []);
    }

    public static SocialChallengeExecution[] All => [NoonOnTheSquareSolvedByExplorerAndSecondExplorer, NoonOnTheSquareSolvedBySecondAndThirdExplorer];
}
