using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubMembership;

public sealed class ClubMembershipService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubMembershipService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RequestToJoinAsync(Guid userId, Guid clubId)
    {
        var club = await GetClubAsync(clubId);

        club.RequestToJoin(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task WithdrawRequestAsync(Guid userId, Guid clubId)
    {
        var club = await GetClubAsync(clubId);

        club.WithdrawRequest(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptRequestAsync(Guid userId, Guid clubId, Guid requesterId)
    {
        var club = await GetClubAsync(clubId);

        club.AcceptRequest(userId, requesterId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectRequestAsync(Guid userId, Guid clubId, Guid requesterId)
    {
        var club = await GetClubAsync(clubId);

        club.RejectRequest(userId, requesterId);
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
