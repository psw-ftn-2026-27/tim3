using Maps.Core.UserPositions;

namespace Maps.Core.UserPositioning;

public sealed class UserPositioningService
{
    private readonly IUserPositionRepository _userPositionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserPositioningService(IUserPositionRepository userPositionRepository, IUnitOfWork unitOfWork)
    {
        _userPositionRepository = userPositionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SetAsync(Guid userId, SetUserPositionDto dto)
    {
        var coordinates = new Coordinates(dto.Latitude, dto.Longitude);
        var setAt = DateTime.UtcNow;

        var userPosition = await _userPositionRepository.GetByUserIdAsync(userId);
        if (userPosition is null)
        {
            _userPositionRepository.Add(new UserPosition(userId, coordinates, setAt));
        }
        else
        {
            userPosition.MoveTo(coordinates, setAt);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
