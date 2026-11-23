using Games.Domain.Locations;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallengeHunt
{
    private const double MaximumActivationDistance = 50;
    private const double MaximumDiscoveryDistance = 5;
    public const double MaximumExtraMetres = 100;

    public LocationChallengeParticipation Activate(Guid explorerId, LocationChallenge challenge,
        IReadOnlyList<LocationChallengeParticipation> explorerParticipations, Location position, double extraMetres)
    {
        if (position.DistanceTo(challenge.StartLocation) > MaximumActivationDistance)
        {
            throw new DomainException($"A location challenge can be activated only within {MaximumActivationDistance} m of its start location.");
        }
        if (explorerParticipations.Any(participation => participation.LocationChallengeId == challenge.Id
            && participation.Status == LocationChallengeParticipationStatus.Solved))
        {
            throw new DomainException("A solved location challenge cannot be activated again.");
        }
        if (explorerParticipations.Any(participation => participation.Status == LocationChallengeParticipationStatus.Active))
        {
            throw new DomainException("Only one location challenge can be active at a time.");
        }
        if (extraMetres < 0 || extraMetres > MaximumExtraMetres)
        {
            throw new DomainException($"A search circle can be enlarged by between 0 and {MaximumExtraMetres} m.");
        }

        var farthestPointDistance = challenge.HiddenPoints
            .Max(hiddenPoint => challenge.StartLocation.DistanceTo(hiddenPoint.Location));
        var searchCircle = new SearchCircle(challenge.StartLocation, farthestPointDistance + extraMetres);

        return new LocationChallengeParticipation(explorerId, challenge.Id, searchCircle);
    }

    public void CheckLocation(LocationChallenge challenge, LocationChallengeParticipation participation,
        Location position, DateTime time)
    {
        if (participation.LocationChallengeId != challenge.Id)
        {
            throw new DomainException("The participation does not belong to this location challenge.");
        }
        if (participation.Status == LocationChallengeParticipationStatus.Solved)
        {
            throw new DomainException("A solved location challenge cannot be continued.");
        }

        var discoveredPoint = challenge.HiddenPoints.FirstOrDefault(hiddenPoint =>
            !participation.DiscoveredPointIds.Contains(hiddenPoint.Id)
            && position.DistanceTo(hiddenPoint.Location) <= MaximumDiscoveryDistance);
        if (discoveredPoint is null)
        {
            throw new DomainException("You are not at any hidden location of this challenge.");
        }

        participation.Discover(discoveredPoint.Id, challenge.HiddenPoints.Count,
            challenge.ExperienceGained, time);
    }
}
