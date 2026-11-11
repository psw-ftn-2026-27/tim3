using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.ClubManagement;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal sealed class ClubReadRepository : IClubReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ClubReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<ClubDto>> GetPageAsync(int page, int pageSize)
    {
        var clubs = _dbContext.Clubs
            .OrderBy(club => club.Name)
            .ThenBy(club => club.Id);

        var totalCount = await clubs.CountAsync();
        var items = await ProjectToDtos(clubs)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<ClubDto>(items, totalCount);
    }

    private static IQueryable<ClubDto> ProjectToDtos(IQueryable<Club> clubs)
    {
        return clubs
            .Select(club => new ClubDto(
                club.Id,
                club.OwnerId,
                club.Name,
                club.Description,
                club.Images));
    }
}
