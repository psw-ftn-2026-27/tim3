using Shared.Domain.Exceptions;

namespace Games.Domain.Locations;

public sealed record Location
{
    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("A location requires a latitude between -90 and 90.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("A location requires a longitude between -180 and 180.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }
}
