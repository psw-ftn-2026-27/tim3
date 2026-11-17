using Maps.Core.UserPositions;
using Microsoft.EntityFrameworkCore;

namespace Maps.Infrastructure.Persistence.UserPositions;

internal sealed class UserPositionRepository : IUserPositionRepository
{
    private readonly MapsDbContext _dbContext;

    public UserPositionRepository(MapsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserPosition?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserPositions.SingleOrDefaultAsync(userPosition => userPosition.UserId == userId);
    }

    public void Add(UserPosition userPosition)
    {
        _dbContext.UserPositions.Add(userPosition);
    }
}
