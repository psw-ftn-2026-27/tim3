using Maps.Core.UserPositions;
using Shared.Domain.Exceptions;

namespace Maps.Core.UserPositioning;

public sealed class UserPositioningQueries
{
    private readonly IUserPositionReadRepository _userPositionReadRepository;

    public UserPositioningQueries(IUserPositionReadRepository userPositionReadRepository)
    {
        _userPositionReadRepository = userPositionReadRepository;
    }

    public async Task<UserPositionDto> GetMyPositionAsync(Guid userId)
    {
        return await _userPositionReadRepository.GetByUserIdAsync(userId)
            ?? throw new NotFoundException($"User {userId} has not set a position yet.");
    }
}
