using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record GroupSize
{
    public int From { get; private init; }
    public int To { get; private init; }

    private GroupSize() { }

    public GroupSize(int from, int to)
    {
        if (from < 1)
        {
            throw new DomainException("A group size requires at least one person.");
        }
        if (to < from)
        {
            throw new DomainException("A group size cannot end below its start.");
        }

        From = from;
        To = to;
    }
}
