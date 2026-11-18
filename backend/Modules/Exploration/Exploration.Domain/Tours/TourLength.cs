using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record TourLength
{
    public double Kilometres { get; private init; }
    public bool IsStraightLine { get; private init; }

    private TourLength() { }

    public TourLength(double kilometres, bool isStraightLine)
    {
        if (kilometres < 0)
        {
            throw new DomainException("A tour length cannot be negative.");
        }

        Kilometres = kilometres;
        IsStraightLine = isStraightLine;
    }
}
