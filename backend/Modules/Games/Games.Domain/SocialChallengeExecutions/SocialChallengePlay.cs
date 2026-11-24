using Games.Domain.Locations;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallengeExecutions;

// Playing a social challenge needs both the challenge (status, period, location, radius,
// people required, XP) and its execution, so the rules that span the two live here.
public sealed class SocialChallengePlay
{
    public static readonly TimeSpan MaxPositionAge = TimeSpan.FromSeconds(30);

    // P1, P2, P3, P5: returns the execution the explorer is now in, either the current one or a new
    // one when there is none or the current one has expired.
    public SocialChallengeExecution Activate(SocialChallenge challenge, SocialChallengeExecution? currentExecution,
        Guid explorerId, Location position, DateTime positionSetAt, DateTime now)
    {
        if (currentExecution is not null)
        {
            EnsureSameChallenge(challenge, currentExecution);
        }
        if (challenge.Status != SocialChallengeStatus.Published || challenge.Location is null)
        {
            throw new DomainException("This social challenge is not available.");
        }
        EnsureFreshPosition(positionSetAt, now);
        if (!challenge.ActivePeriod.Contains(now))
        {
            throw new DomainException("This social challenge is not active at this time of day.");
        }
        if (position.DistanceTo(challenge.Location) > challenge.GatheringRadius)
        {
            throw new DomainException("You are outside the gathering circle.");
        }

        var execution = currentExecution;
        if (execution is not null && execution.IsExpired(now))
        {
            execution.Expire(now);
            execution = null;
        }
        execution ??= new SocialChallengeExecution(challenge.Id, now);

        execution.Join(explorerId, now);
        return execution;
    }

    // P2, P4, P6: records the report and solves the execution once enough explorers are active.
    public void Report(SocialChallenge challenge, SocialChallengeExecution execution, Guid explorerId,
        Location position, DateTime positionSetAt, DateTime now, IReadOnlyCollection<Guid> rewardedToday)
    {
        EnsureSameChallenge(challenge, execution);
        if (challenge.Location is null)
        {
            throw new DomainException("This social challenge is not available.");
        }
        EnsureFreshPosition(positionSetAt, now);

        var inRange = position.DistanceTo(challenge.Location) <= challenge.GatheringRadius;
        execution.Report(explorerId, inRange, now);

        if (execution.ActiveCount(now) >= challenge.PeopleRequired)
        {
            execution.Solve(now, challenge.ExperienceGained, rewardedToday);
        }
    }

    private static void EnsureFreshPosition(DateTime positionSetAt, DateTime now)
    {
        if (now - positionSetAt > MaxPositionAge)
        {
            throw new DomainException("Please update your location.");
        }
    }

    private static void EnsureSameChallenge(SocialChallenge challenge, SocialChallengeExecution execution)
    {
        if (execution.ChallengeId != challenge.Id)
        {
            throw new DomainException("The execution belongs to another social challenge.");
        }
    }
}
