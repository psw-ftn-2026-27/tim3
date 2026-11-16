using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubRoster;

public sealed class ClubRosterQueries
{
    private readonly IClubReadRepository _clubReadRepository;
    private readonly IIdentityApi _identityApi;

    public ClubRosterQueries(IClubReadRepository clubReadRepository, IIdentityApi identityApi)
    {
        _clubReadRepository = clubReadRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<ClubUserDto>> SearchInviteesAsync(Guid userId, string text, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = await _identityApi.SearchUsersAsync(text, userId, page, pageSize);

        var items = users.Items
            .Select(user => new ClubUserDto(user.UserId, user.DisplayName))
            .ToList();
        return new PageResult<ClubUserDto>(items, users.TotalCount);
    }

    // Only the club's owner sees its pending invitations; every other user gets an empty list.
    public async Task<List<ClubUserDto>> GetPendingInvitationsAsync(Guid userId, Guid clubId)
    {
        var club = await _clubReadRepository.GetInvitationsAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        if (club.OwnerId != userId)
        {
            return [];
        }

        var names = await _identityApi.GetDisplayNamesAsync(club.InviteeIds);
        var namesById = names.ToDictionary(name => name.UserId, name => name.DisplayName);
        return club.InviteeIds
            .Select(inviteeId => new ClubUserDto(inviteeId, namesById.GetValueOrDefault(inviteeId, "")))
            .ToList();
    }

    public Task<List<ReceivedInvitationDto>> GetMyInvitationsAsync(Guid userId)
    {
        return _clubReadRepository.GetInvitationsForAsync(userId);
    }
}
