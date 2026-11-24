using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallengeExecutions;

// One execution of a social challenge, shared by every explorer who gathers for it.
// The challenge itself is referred to by ID only.
public sealed class SocialChallengeExecution : AggregateRoot
{
    public static readonly TimeSpan ActiveWindow = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan ExpiryTime = TimeSpan.FromMinutes(5);

    private readonly List<SocialChallengeParticipant> _participants = [];

    public Guid ChallengeId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public SocialChallengeExecutionStatus Status { get; private set; }
    public DateTime? SolvedAt { get; private set; }
    public IReadOnlyList<SocialChallengeParticipant> Participants => _participants;

    private SocialChallengeExecution() { }

    public SocialChallengeExecution(Guid challengeId, DateTime startedAt) : base(Guid.NewGuid())
    {
        ChallengeId = challengeId;
        StartedAt = startedAt;
        Status = SocialChallengeExecutionStatus.Active;
    }

    // P3: an explorer already in the execution is not added again.
    public void Join(Guid explorerId, DateTime now)
    {
        EnsureCanContinue(now);
        if (_participants.Any(participant => participant.ExplorerId == explorerId))
        {
            return;
        }

        _participants.Add(new SocialChallengeParticipant(explorerId, now));
    }

    public void Report(Guid explorerId, bool inRange, DateTime now)
    {
        EnsureCanContinue(now);
        var participant = _participants.FirstOrDefault(participant => participant.ExplorerId == explorerId)
            ?? throw new DomainException("You have not joined this social challenge.");

        participant.Report(inRange, now, ActiveWindow);
    }

    // P4: only explorers who reported from the circle within the active window are active.
    public int ActiveCount(DateTime now) =>
        _participants.Count(participant => participant.IsActive(now, ActiveWindow));

    // P5: an execution in which nobody reported for the expiry time has expired.
    public bool IsExpired(DateTime now)
    {
        if (Status == SocialChallengeExecutionStatus.Expired)
        {
            return true;
        }
        if (Status == SocialChallengeExecutionStatus.Solved)
        {
            return false;
        }
        var lastReportAt = _participants.Count == 0
            ? StartedAt
            : _participants.Max(participant => participant.LastReportAt);
        return now - lastReportAt > ExpiryTime;
    }

    public void Expire(DateTime now)
    {
        if (Status != SocialChallengeExecutionStatus.Active || !IsExpired(now))
        {
            throw new DomainException("Only an active execution in which nobody reported for 5 minutes can expire.");
        }

        Status = SocialChallengeExecutionStatus.Expired;
    }

    // P6, P7, P8, P10: every participant gets the moment of the solve and a share of the challenge's
    // XP per person by their share of active time, rounded down; one already rewarded today gets none.
    public void Solve(DateTime now, int challengeExperience, IReadOnlyCollection<Guid> rewardedToday)
    {
        EnsureCanContinue(now);

        foreach (var participant in _participants)
        {
            participant.Close(now, ActiveWindow);
            var experienceGained = rewardedToday.Contains(participant.ExplorerId)
                ? 0
                : challengeExperience * ExperiencePercentFor(participant.ActiveShareUntil(now)) / 100;
            participant.Award(experienceGained);
        }

        Status = SocialChallengeExecutionStatus.Solved;
        SolvedAt = now;
    }

    private static int ExperiencePercentFor(double activeShare)
    {
        if (activeShare >= 0.70)
        {
            return 100;
        }
        if (activeShare >= 0.35)
        {
            return 50;
        }
        return 10;
    }

    private void EnsureCanContinue(DateTime now)
    {
        if (Status == SocialChallengeExecutionStatus.Solved)
        {
            throw new DomainException("This social challenge execution is already solved.");
        }
        if (IsExpired(now))
        {
            throw new DomainException("This social challenge execution has expired.");
        }
    }
}
