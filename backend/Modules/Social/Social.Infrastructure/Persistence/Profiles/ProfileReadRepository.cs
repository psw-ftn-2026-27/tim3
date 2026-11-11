using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.FollowRequesting;
using Social.Application.Profiles;
using Social.Domain.Profiles;

namespace Social.Infrastructure.Persistence.Profiles;

internal sealed class ProfileReadRepository : IProfileReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ProfileReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<SentFollowRequestRowDto>> GetSentRequestsAsync(Guid senderId, int page, int pageSize)
    {
        var sent = _dbContext.Profiles
            .Where(profile => profile.UserId == senderId)
            .SelectMany(profile => profile.SentRequests)
            .OrderByDescending(request => request.SentAt)
            .ThenBy(request => request.Id);

        var totalCount = await sent.CountAsync();
        var items = await sent
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(request => new SentFollowRequestRowDto(request.Id, request.RecipientId, request.SentAt, request.Status))
            .ToListAsync();

        return new PageResult<SentFollowRequestRowDto>(items, totalCount);
    }

    public async Task<PageResult<IncomingFollowRequestRowDto>> GetIncomingRequestsAsync(Guid recipientId, int page, int pageSize)
    {
        var incoming = _dbContext.Profiles
            .SelectMany(profile => profile.SentRequests, (profile, request) => new { profile.UserId, Request = request })
            .Where(sent => sent.Request.RecipientId == recipientId && sent.Request.Status == FollowRequestStatus.Pending)
            .OrderByDescending(sent => sent.Request.SentAt)
            .ThenBy(sent => sent.Request.Id);

        var totalCount = await incoming.CountAsync();
        var items = await incoming
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(sent => new IncomingFollowRequestRowDto(sent.Request.Id, sent.UserId, sent.Request.SentAt))
            .ToListAsync();

        return new PageResult<IncomingFollowRequestRowDto>(items, totalCount);
    }
}
