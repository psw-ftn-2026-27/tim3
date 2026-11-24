using Games.Application.LocationChallengeParticipations;
using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallengeParticipations;

internal sealed class LocationChallengeParticipationRepository : ILocationChallengeParticipationRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeParticipationRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<LocationChallengeParticipation>> GetByExplorerAsync(Guid explorerId)
    {
        return _dbContext.LocationChallengeParticipations
            .Where(participation => participation.ExplorerId == explorerId)
            .ToListAsync();
    }

    public Task<LocationChallengeParticipation?> GetAsync(Guid explorerId, Guid locationChallengeId)
    {
        return _dbContext.LocationChallengeParticipations.FirstOrDefaultAsync(participation =>
            participation.ExplorerId == explorerId && participation.LocationChallengeId == locationChallengeId);
    }

    public void Add(LocationChallengeParticipation participation)
    {
        _dbContext.LocationChallengeParticipations.Add(participation);
    }
}
