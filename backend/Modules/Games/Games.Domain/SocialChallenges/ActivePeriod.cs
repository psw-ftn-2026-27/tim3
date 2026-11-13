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
}
