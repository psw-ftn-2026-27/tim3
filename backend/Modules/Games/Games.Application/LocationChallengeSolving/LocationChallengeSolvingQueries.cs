using Games.Application.LocationChallengeParticipations;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeSolving;

public sealed class LocationChallengeSolvingQueries
{
    private readonly ILocationChallengeParticipationReadRepository _participationReadRepository;

    public LocationChallengeSolvingQueries(ILocationChallengeParticipationReadRepository participationReadRepository)
    {
        _participationReadRepository = participationReadRepository;
    }

    public Task<PageResult<LocationChallengeToSolveDto>> GetAllAsync(Guid explorerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _participationReadRepository.GetPageAsync(explorerId, page, pageSize);
    }

    public async Task<LocationChallengeHuntDto> GetByIdAsync(Guid explorerId, Guid locationChallengeId)
    {
        var locationChallenge = await _participationReadRepository.GetByIdAsync(explorerId, locationChallengeId);
        if (locationChallenge is null)
        {
            throw new NotFoundException($"Location challenge {locationChallengeId} does not exist.");
        }

        return locationChallenge;
    }
}
