using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeRepository : ILocationChallengeRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LocationChallenge?> GetByIdAsync(Guid id)
    {
        return _dbContext.LocationChallenges.FirstOrDefaultAsync(locationChallenge => locationChallenge.Id == id);
    }

    public void Add(LocationChallenge locationChallenge)
    {
        _dbContext.LocationChallenges.Add(locationChallenge);
    }

    public void Remove(LocationChallenge locationChallenge)
    {
        _dbContext.LocationChallenges.Remove(locationChallenge);
    }
}
