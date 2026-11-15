using Maps.Core.UserPositioning;

namespace Maps.Core.UserPositions;

public interface IUserPositionReadRepository
{
    Task<UserPositionDto?> GetByUserIdAsync(Guid userId);
}
