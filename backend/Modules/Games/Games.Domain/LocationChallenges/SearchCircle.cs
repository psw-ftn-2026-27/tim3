using Games.Domain.Locations;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed record SearchCircle
{
    public Location Center { get; private init; }
    public double Radius { get; private init; }

    private SearchCircle()
    {
        Center = null!;
    }

    public SearchCircle(Location center, double radius)
    {
        if (radius < 0)
        {
            throw new DomainException("A search circle requires a radius of at least 0 m.");
        }

        Center = center;
        Radius = radius;
    }
}
