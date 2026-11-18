using Games.Application.LocationChallengeAuthoring;
using Shared.Domain;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<PageResult<LocationChallengeSummaryDto>> GetPageAsync(int page, int pageSize);
    Task<LocationChallengeDto?> GetByIdAsync(Guid id);
}
