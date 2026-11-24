using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed record ActivePeriod
{
    public TimeOnly Start { get; private init; }
    public TimeOnly End { get; private init; }

    private ActivePeriod() { }

    public ActivePeriod(TimeOnly start, TimeOnly end)
    {
        if (start == end)
        {
            throw new DomainException("An active period requires a start and an end that differ.");
        }

        Start = start;
        End = end;
    }

    // The period is a time of day on the server's local clock, while moments are stored in UTC.
    // A period that ends before it starts spans midnight.
    public bool Contains(DateTime moment)
    {
        return TimeOnly.FromDateTime(moment.ToLocalTime()).IsBetween(Start, End);
    }

    // The day a moment belongs to is the local day on which the latest period started at or before
    // it, so a period spanning midnight belongs to the day it started. Returned as UTC bounds,
    // from inclusive to exclusive.
    public (DateTime From, DateTime To) DayOf(DateTime moment)
    {
        var localMoment = moment.ToLocalTime();
        var startDay = TimeOnly.FromDateTime(localMoment) >= Start
            ? DateOnly.FromDateTime(localMoment)
            : DateOnly.FromDateTime(localMoment).AddDays(-1);

        return (startDay.ToDateTime(Start, DateTimeKind.Local).ToUniversalTime(),
            startDay.AddDays(1).ToDateTime(Start, DateTimeKind.Local).ToUniversalTime());
    }
}
