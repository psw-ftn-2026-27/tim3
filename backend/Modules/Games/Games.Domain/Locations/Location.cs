using Shared.Domain.Exceptions;

namespace Games.Domain.Locations;

public sealed record Location
{
    private const double EarthRadiusInMetres = 6_371_000;

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

    // A location given as two optional coordinates: both or neither, never only one.
    public static Location? FromOptionalCoordinates(double? latitude, double? longitude)
    {
        if (latitude is null && longitude is null)
        {
            return null;
        }
        if (latitude is null || longitude is null)
        {
            throw new DomainException("A location requires both a latitude and a longitude.");
        }

        return new Location(latitude.Value, longitude.Value);
    }

    // The great-circle distance in metres (haversine formula on a spherical Earth).
    public double DistanceTo(Location other)
    {
        var latitudeDelta = ToRadians(other.Latitude - Latitude);
        var longitudeDelta = ToRadians(other.Longitude - Longitude);
        var a = Math.Pow(Math.Sin(latitudeDelta / 2), 2)
            + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude))
            * Math.Pow(Math.Sin(longitudeDelta / 2), 2);
        return 2 * EarthRadiusInMetres * Math.Asin(Math.Sqrt(a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
