using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubRoles;

public sealed class ClubRolesService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubRolesService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PromoteToModeratorAsync(Guid userId, Guid clubId, Guid memberId)
    {
        var club = await GetClubAsync(clubId);

        club.PromoteToModerator(userId, memberId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task TransferOwnershipAsync(Guid userId, Guid clubId, Guid newOwnerId)
    {
        var club = await GetClubAsync(clubId);

        club.TransferOwnership(userId, newOwnerId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetClubAsync(Guid clubId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }
}
