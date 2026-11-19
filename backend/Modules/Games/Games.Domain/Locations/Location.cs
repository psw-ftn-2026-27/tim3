using Shared.Domain.Exceptions;

namespace Games.Domain.Locations;

public sealed record Location
{
    private const double EarthRadiusInMetres = 6371000;

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

    public double DistanceTo(Location other)
    {
        var latitudeDifference = ToRadians(other.Latitude - Latitude);
        var longitudeDifference = ToRadians(other.Longitude - Longitude);

        var haversine = Math.Sin(latitudeDifference / 2) * Math.Sin(latitudeDifference / 2)
            + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude))
            * Math.Sin(longitudeDifference / 2) * Math.Sin(longitudeDifference / 2);

        return 2 * EarthRadiusInMetres * Math.Asin(Math.Sqrt(haversine));
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
