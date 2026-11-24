using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeRepository : ISocialChallengeRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<SocialChallenge?> GetByIdAsync(Guid id)
    {
        return _dbContext.SocialChallenges.FirstOrDefaultAsync(socialChallenge => socialChallenge.Id == id);
    }

    public void Add(SocialChallenge socialChallenge)
    {
        _dbContext.SocialChallenges.Add(socialChallenge);
    }

    public void Remove(SocialChallenge socialChallenge)
    {
        _dbContext.SocialChallenges.Remove(socialChallenge);
    }
}
