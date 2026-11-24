using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallengePlaying;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallengeExecutions;
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

    public async Task<PageResult<PublishedSocialChallengeDto>> GetPublishedAsync(Guid explorerId, int page, int pageSize)
    {
        var socialChallenges = _dbContext.SocialChallenges
            .Where(socialChallenge => socialChallenge.Status == SocialChallengeStatus.Published
                && socialChallenge.Location != null)
            .OrderBy(socialChallenge => socialChallenge.Name)
            .ThenBy(socialChallenge => socialChallenge.Id);

        var totalCount = await socialChallenges.CountAsync();
        var items = await socialChallenges
            .Select(socialChallenge => new PublishedSocialChallengeDto(
                socialChallenge.Id,
                socialChallenge.Name,
                socialChallenge.ActivePeriod.Start,
                socialChallenge.ActivePeriod.End,
                socialChallenge.PeopleRequired,
                socialChallenge.Location!.Latitude,
                socialChallenge.Location.Longitude,
                _dbContext.SocialChallengeExecutions.Count(execution =>
                    execution.ChallengeId == socialChallenge.Id
                    && execution.Status == SocialChallengeExecutionStatus.Solved
                    && execution.Participants.Any(participant => participant.ExplorerId == explorerId))))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<PublishedSocialChallengeDto>(items, totalCount);
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
                socialChallenge.EndMessage,
                socialChallenge.Location == null ? null : socialChallenge.Location.Latitude,
                socialChallenge.Location == null ? null : socialChallenge.Location.Longitude));
    }
}
