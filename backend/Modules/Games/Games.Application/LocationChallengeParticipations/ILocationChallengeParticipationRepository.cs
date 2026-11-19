using Games.Domain.LocationChallenges;

namespace Games.Application.LocationChallengeParticipations;

public interface ILocationChallengeParticipationRepository
{
    Task<List<LocationChallengeParticipation>> GetByExplorerAsync(Guid explorerId);
    Task<LocationChallengeParticipation?> GetAsync(Guid explorerId, Guid locationChallengeId);
    void Add(LocationChallengeParticipation participation);
}
