using Games.Application.LocationChallengeParticipations;
using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Maps.Contracts;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeSolving;

public sealed class LocationChallengeSolvingService
{
    private const double MaximumExtraMetres = 100;

    private readonly ILocationChallengeRepository _locationChallengeRepository;
    private readonly ILocationChallengeParticipationRepository _participationRepository;
    private readonly LocationChallengeHunt _hunt;
    private readonly IMapsApi _mapsApi;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengeSolvingService(
        ILocationChallengeRepository locationChallengeRepository,
        ILocationChallengeParticipationRepository participationRepository,
        LocationChallengeHunt hunt,
        IMapsApi mapsApi,
        IUnitOfWork unitOfWork)
    {
        _locationChallengeRepository = locationChallengeRepository;
        _participationRepository = participationRepository;
        _hunt = hunt;
        _mapsApi = mapsApi;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ActivateAsync(Guid explorerId, Guid locationChallengeId)
    {
        var position = await GetPositionAsync(explorerId);
        var locationChallenge = await GetLocationChallengeAsync(locationChallengeId);
        var explorerParticipations = await _participationRepository.GetByExplorerAsync(explorerId);
        var extraMetres = Random.Shared.NextDouble() * MaximumExtraMetres;

        var participation = _hunt.Activate(explorerId, locationChallenge, explorerParticipations,
            position, extraMetres);
        _participationRepository.Add(participation);
        await _unitOfWork.SaveChangesAsync();

        return participation.Id;
    }

    public async Task CheckLocationAsync(Guid explorerId, Guid locationChallengeId)
    {
        var position = await GetPositionAsync(explorerId);
        var locationChallenge = await GetLocationChallengeAsync(locationChallengeId);
        var participation = await _participationRepository.GetAsync(explorerId, locationChallengeId);
        if (participation is null)
        {
            throw new NotFoundException($"You have not activated location challenge {locationChallengeId}.");
        }

        _hunt.CheckLocation(locationChallenge, participation, position, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Location> GetPositionAsync(Guid explorerId)
    {
        var position = await _mapsApi.GetCurrentPositionAsync(explorerId);
        if (position is null)
        {
            throw new DomainException("Set your position on the map first.");
        }
        return new Location(position.Latitude, position.Longitude);
    }

    private async Task<LocationChallenge> GetLocationChallengeAsync(Guid locationChallengeId)
    {
        var locationChallenge = await _locationChallengeRepository.GetByIdAsync(locationChallengeId);
        if (locationChallenge is null)
        {
            throw new NotFoundException($"Location challenge {locationChallengeId} does not exist.");
        }
        return locationChallenge;
    }
}
