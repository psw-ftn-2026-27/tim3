using Identity.Contracts;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubRoster;

public sealed class ClubRosterService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public ClubRosterService(IClubRepository clubRepository, IUnitOfWork unitOfWork, IIdentityApi identityApi)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task InviteAsync(Guid userId, Guid clubId, Guid inviteeId)
    {
        if (!await _identityApi.ExistsAsync(inviteeId))
        {
            throw new NotFoundException($"User {inviteeId} does not exist.");
        }
        var club = await GetClubAsync(clubId);

        club.Invite(userId, inviteeId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task WithdrawInvitationAsync(Guid userId, Guid clubId, Guid inviteeId)
    {
        var club = await GetClubAsync(clubId);

        club.WithdrawInvitation(userId, inviteeId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptInvitationAsync(Guid userId, Guid clubId)
    {
        var club = await GetClubAsync(clubId);

        club.AcceptInvitation(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeclineInvitationAsync(Guid userId, Guid clubId)
    {
        var club = await GetClubAsync(clubId);

        club.DeclineInvitation(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid userId, Guid clubId, Guid memberId)
    {
        var club = await GetClubAsync(clubId);

        club.RemoveMember(userId, memberId);
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
