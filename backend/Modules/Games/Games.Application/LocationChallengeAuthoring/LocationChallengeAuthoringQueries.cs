using Games.Application.LocationChallenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeAuthoring;

public sealed class LocationChallengeAuthoringQueries
{
    private readonly ILocationChallengeReadRepository _locationChallengeReadRepository;

    public LocationChallengeAuthoringQueries(ILocationChallengeReadRepository locationChallengeReadRepository)
    {
        _locationChallengeReadRepository = locationChallengeReadRepository;
    }

    public Task<PageResult<LocationChallengeSummaryDto>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _locationChallengeReadRepository.GetPageAsync(page, pageSize);
    }

    public async Task<LocationChallengeDto> GetByIdAsync(Guid locationChallengeId)
    {
        var locationChallenge = await _locationChallengeReadRepository.GetByIdAsync(locationChallengeId);
        if (locationChallenge is null)
        {
            throw new NotFoundException($"Location challenge {locationChallengeId} does not exist.");
        }

        return locationChallenge;
    }
}
