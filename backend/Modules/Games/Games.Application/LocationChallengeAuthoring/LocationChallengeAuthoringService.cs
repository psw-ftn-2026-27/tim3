using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Games.Domain.Locations;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeAuthoring;

public sealed class LocationChallengeAuthoringService
{
    private readonly ILocationChallengeRepository _locationChallengeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LocationChallengeAuthoringService(
        ILocationChallengeRepository locationChallengeRepository,
        IUnitOfWork unitOfWork)
    {
        _locationChallengeRepository = locationChallengeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(CreateLocationChallengeDto dto)
    {
        var startLocation = new Location(dto.StartLatitude, dto.StartLongitude);
        var locationChallenge = new LocationChallenge(dto.Name, startLocation,
            dto.ExperienceGained, ToHiddenPoints(dto.HiddenPoints));

        _locationChallengeRepository.Add(locationChallenge);
        await _unitOfWork.SaveChangesAsync();

        return locationChallenge.Id;
    }

    public async Task UpdateAsync(Guid locationChallengeId, UpdateLocationChallengeDto dto)
    {
        var locationChallenge = await GetLocationChallengeAsync(locationChallengeId);

        var startLocation = new Location(dto.StartLatitude, dto.StartLongitude);
        locationChallenge.Update(dto.Name, startLocation,
            dto.ExperienceGained, ToHiddenPoints(dto.HiddenPoints));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid locationChallengeId)
    {
        var locationChallenge = await GetLocationChallengeAsync(locationChallengeId);

        _locationChallengeRepository.Remove(locationChallenge);
        await _unitOfWork.SaveChangesAsync();
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

    private static List<HiddenPoint> ToHiddenPoints(List<CreateHiddenPointDto> dtos)
    {
        return dtos
            .Select(dto => new HiddenPoint(dto.Photo, new Location(dto.Latitude, dto.Longitude)))
            .ToList();
    }
}
