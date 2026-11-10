using Shared.Domain;
using Social.Application.ClubManagement;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<PageResult<ClubDto>> GetPageAsync(int page, int pageSize);
}
