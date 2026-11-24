using FluentAssertions;
using Games.Domain.SocialChallengeExecutions;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Unit.SocialChallengeExecutions;

public class SocialChallengeExecutionTests
{
    private static readonly DateTime StartedAt = new(2026, 11, 24, 11, 0, 0, DateTimeKind.Utc);
    private const int ChallengeExperience = 300;

    private static SocialChallengeExecution CreateExecutionJoinedBy(params Guid[] explorerIds)
    {
        var execution = new SocialChallengeExecution(Guid.NewGuid(), StartedAt);
        foreach (var explorerId in explorerIds)
        {
            execution.Join(explorerId, StartedAt);
        }
        return execution;
    }

    [Fact]
    public void Only_explorers_who_reported_from_the_circle_in_the_last_30_seconds_are_active()
    {
        var execution = CreateExecutionJoinedBy(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer, WellKnownUsers.ThirdExplorer);
        execution.Report(WellKnownUsers.SecondExplorer, false, StartedAt.AddSeconds(20));
        execution.Report(WellKnownUsers.ThirdExplorer, true, StartedAt.AddSeconds(25));

        var activeCount = execution.ActiveCount(StartedAt.AddSeconds(31));

        activeCount.Should().Be(1);
    }

    [Fact]
    public void Explorer_who_reports_from_the_circle_again_is_active_again()
    {
        var execution = CreateExecutionJoinedBy(WellKnownUsers.Explorer);
        execution.Report(WellKnownUsers.Explorer, false, StartedAt.AddSeconds(5));

        execution.Report(WellKnownUsers.Explorer, true, StartedAt.AddSeconds(10));

        execution.ActiveCount(StartedAt.AddSeconds(10)).Should().Be(1);
    }

    [Theory]
    [InlineData(6_800, 30)]
    [InlineData(7_000, 150)]
    [InlineData(13_800, 150)]
    [InlineData(14_000, 300)]
    public void Share_of_active_time_decides_the_share_of_experience(int activeMilliseconds, int expectedExperience)
    {
        var execution = CreateExecutionJoinedBy(WellKnownUsers.Explorer);
        execution.Report(WellKnownUsers.Explorer, false, StartedAt.AddMilliseconds(activeMilliseconds));

        execution.Solve(StartedAt.AddSeconds(20), ChallengeExperience, []);

        execution.Participants.Single().ExperienceGained.Should().Be(expectedExperience);
    }

    [Fact]
    public void Explorer_already_rewarded_today_gets_the_solve_recorded_without_experience()
    {
        var execution = CreateExecutionJoinedBy(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        var solvedAt = StartedAt.AddSeconds(10);

        execution.Solve(solvedAt, ChallengeExperience, [WellKnownUsers.Explorer]);

        execution.SolvedAt.Should().Be(solvedAt);
        execution.Participants.Single(participant => participant.ExplorerId == WellKnownUsers.Explorer)
            .ExperienceGained.Should().Be(0);
        execution.Participants.Single(participant => participant.ExplorerId == WellKnownUsers.SecondExplorer)
            .ExperienceGained.Should().Be(ChallengeExperience);
    }
}
