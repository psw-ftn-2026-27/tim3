using Shared.Domain;
using Social.Application.Clubs;

namespace Social.Application.ClubManagement;

public sealed class ClubManagementQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubManagementQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public Task<PageResult<ClubDto>> GetPageAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _clubReadRepository.GetPageAsync(page, pageSize);
    }
}
