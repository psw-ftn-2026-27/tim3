using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class HiddenPoint : Entity
{
    public string Photo { get; private set; }
    public Location Location { get; private set; }

    private HiddenPoint()
    {
        Photo = null!;
        Location = null!;
    }

    public HiddenPoint(string photo, Location? location) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(photo))
        {
            throw new DomainException("A hidden point requires a photo.");
        }
        if (location is null)
        {
            throw new DomainException("A hidden point requires a location.");
        }

        Photo = photo;
        Location = location;
    }
}
