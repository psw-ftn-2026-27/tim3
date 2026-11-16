using Shared.Domain;
using Social.Application.ClubManagement;
using Social.Application.ClubMembership;
using Social.Application.ClubRoster;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<PageResult<ClubDto>> GetPageAsync(int page, int pageSize);
    Task<ClubDetailsRowDto?> GetDetailsAsync(Guid clubId);
    Task<ClubInvitationsRowDto?> GetInvitationsAsync(Guid clubId);
    Task<List<ReceivedInvitationDto>> GetInvitationsForAsync(Guid userId);
}
