using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed record Location
{
    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("A latitude is between -90 and 90 degrees.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("A longitude is between -180 and 180 degrees.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }
}
