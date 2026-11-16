using Shared.Domain;
using Social.Application.ClubManagement;
using Social.Application.ClubMembership;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<PageResult<ClubDto>> GetPageAsync(int page, int pageSize);
    Task<ClubDetailsRowDto?> GetDetailsAsync(Guid clubId);
}
