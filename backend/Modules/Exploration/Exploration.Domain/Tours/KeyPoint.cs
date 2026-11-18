using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class KeyPoint : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ImageAddress { get; private set; }
    public GeoLocation Location { get; private set; }
    public string? Secret { get; private set; }
    public bool VisibleWithoutPurchase { get; private set; }
    public int Order { get; internal set; }

    private KeyPoint()
    {
        Name = null!;
        Description = null!;
        ImageAddress = null!;
        Location = null!;
    }

    public KeyPoint(string name, string description, string imageAddress, GeoLocation? location, string? secret, bool visibleWithoutPurchase)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A key point requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A key point requires a description.");
        }
        if (string.IsNullOrWhiteSpace(imageAddress))
        {
            throw new DomainException("A key point requires an image.");
        }
        if (location is null)
        {
            throw new DomainException("A key point requires a location.");
        }

        Name = name;
        Description = description;
        ImageAddress = imageAddress;
        Location = location;
        Secret = string.IsNullOrWhiteSpace(secret) ? null : secret;
        VisibleWithoutPurchase = visibleWithoutPurchase;
    }
}
