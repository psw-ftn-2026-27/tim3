using Maps.Contracts;
using Maps.Core.UserPositions;

namespace Maps.Core;

internal sealed class MapsApi : IMapsApi
{
    private readonly IUserPositionReadRepository _userPositionReadRepository;

    public MapsApi(IUserPositionReadRepository userPositionReadRepository)
    {
        _userPositionReadRepository = userPositionReadRepository;
    }

    public async Task<CurrentPosition?> GetCurrentPositionAsync(Guid userId)
    {
        var position = await _userPositionReadRepository.GetByUserIdAsync(userId);
        return position is null
            ? null
            : new CurrentPosition(position.Latitude, position.Longitude, position.SetAt);
    }
}
