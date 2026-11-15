using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeReadRepository : ISocialChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<SocialChallengeDto>> GetPageAsync(int page, int pageSize)
    {
        var socialChallenges = _dbContext.SocialChallenges
            .OrderBy(socialChallenge => socialChallenge.Name)
            .ThenBy(socialChallenge => socialChallenge.Id);

        var totalCount = await socialChallenges.CountAsync();
        var items = await ProjectToDtos(socialChallenges)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<SocialChallengeDto>(items, totalCount);
    }

    public Task<SocialChallengeDto?> GetByIdAsync(Guid id)
    {
        return ProjectToDtos(_dbContext.SocialChallenges.Where(socialChallenge => socialChallenge.Id == id))
            .FirstOrDefaultAsync();
    }

    private static IQueryable<SocialChallengeDto> ProjectToDtos(IQueryable<SocialChallenge> socialChallenges)
    {
        return socialChallenges
            .Select(socialChallenge => new SocialChallengeDto(
                socialChallenge.Id,
                socialChallenge.Name,
                socialChallenge.Status,
                socialChallenge.ActivePeriod.Start,
                socialChallenge.ActivePeriod.End,
                socialChallenge.PeopleRequired,
                socialChallenge.GatheringRadius,
                socialChallenge.ExperienceGained,
                socialChallenge.EndMessage));
    }
}
