using Games.Application.LocationChallengeSolving;
using Shared.Domain;

namespace Games.Application.LocationChallengeParticipations;

public interface ILocationChallengeParticipationReadRepository
{
    Task<PageResult<LocationChallengeToSolveDto>> GetPageAsync(Guid explorerId, int page, int pageSize);
    Task<LocationChallengeHuntDto?> GetByIdAsync(Guid explorerId, Guid locationChallengeId);
}
