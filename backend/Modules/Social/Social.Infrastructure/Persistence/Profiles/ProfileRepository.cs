using Microsoft.EntityFrameworkCore;
using Social.Application.Profiles;
using Social.Domain.Profiles;

namespace Social.Infrastructure.Persistence.Profiles;

internal sealed class ProfileRepository : IProfileRepository
{
    private readonly SocialDbContext _dbContext;

    public ProfileRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Profile?> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.Profiles.FirstOrDefaultAsync(profile => profile.UserId == userId);
    }

    public Task<Profile?> GetBySentRequestIdAsync(Guid requestId)
    {
        return _dbContext.Profiles.FirstOrDefaultAsync(profile =>
            profile.SentRequests.Any(request => request.Id == requestId));
    }

    public void Add(Profile profile)
    {
        _dbContext.Profiles.Add(profile);
    }
}
