using Maps.Core.UserPositioning;
using Maps.Core.UserPositions;
using Microsoft.EntityFrameworkCore;

namespace Maps.Infrastructure.Persistence.UserPositions;

internal sealed class UserPositionReadRepository : IUserPositionReadRepository
{
    private readonly MapsDbContext _dbContext;

    public UserPositionReadRepository(MapsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserPositionDto?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.UserPositions
            .Where(userPosition => userPosition.UserId == userId)
            .Select(userPosition => new UserPositionDto(
                userPosition.Coordinates.Latitude,
                userPosition.Coordinates.Longitude,
                userPosition.SetAt))
            .SingleOrDefaultAsync();
    }
}
