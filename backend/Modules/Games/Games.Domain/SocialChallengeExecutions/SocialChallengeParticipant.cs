using Shared.Domain;

namespace Games.Domain.SocialChallengeExecutions;

// One explorer in one execution. Created and changed only through its execution.
public sealed class SocialChallengeParticipant : Entity
{
    public Guid ExplorerId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime LastReportAt { get; private set; }
    public bool LastReportInRange { get; private set; }
    public TimeSpan ActiveTime { get; private set; }
    public int? ExperienceGained { get; private set; }

    private SocialChallengeParticipant() { }

    // Joining is possible only from inside the gathering circle, so it counts as a report in range.
    internal SocialChallengeParticipant(Guid explorerId, DateTime joinedAt) : base(Guid.NewGuid())
    {
        ExplorerId = explorerId;
        JoinedAt = joinedAt;
        LastReportAt = joinedAt;
        LastReportInRange = true;
        ActiveTime = TimeSpan.Zero;
    }

    internal void Report(bool inRange, DateTime reportedAt, TimeSpan activeWindow)
    {
        AddActiveTimeUntil(reportedAt, activeWindow);
        LastReportAt = reportedAt;
        LastReportInRange = inRange;
    }

    internal bool IsActive(DateTime now, TimeSpan activeWindow) =>
        LastReportInRange && now - LastReportAt <= activeWindow;

    // The final stretch from the last report to the solve counts like the time after any report.
    internal void Close(DateTime solvedAt, TimeSpan activeWindow)
    {
        AddActiveTimeUntil(solvedAt, activeWindow);
    }

    internal void Award(int experienceGained)
    {
        ExperienceGained = experienceGained;
    }

    // P8: the share of the time from joining to the solve in which the explorer was active.
    // A participant who joined at the very moment of the solve was active the whole (empty) time.
    internal double ActiveShareUntil(DateTime solvedAt)
    {
        var participationTime = solvedAt - JoinedAt;
        if (participationTime <= TimeSpan.Zero)
        {
            return 1;
        }
        return ActiveTime / participationTime;
    }

    // After a report from the circle the explorer stays active for at most the active window (P4).
    private void AddActiveTimeUntil(DateTime moment, TimeSpan activeWindow)
    {
        if (!LastReportInRange)
        {
            return;
        }
        var gap = moment - LastReportAt;
        ActiveTime += gap < activeWindow ? gap : activeWindow;
    }
}
