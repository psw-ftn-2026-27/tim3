using Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

internal sealed class IdentityApi : IIdentityApi
{
    private readonly IdentityModuleDbContext _dbContext;

    public IdentityApi(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserSearchResult> SearchUsersAsync(string text, Guid excludedUserId, int page, int pageSize)
    {
        var normalizedText = text.Trim().ToUpperInvariant();
        var matching = _dbContext.Users
            .Where(user => user.Id != excludedUserId && user.NormalizedUserName!.Contains(normalizedText));

        var totalCount = await matching.CountAsync();
        var users = await matching
            .OrderBy(user => user.NormalizedUserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(user => new { user.Id, user.UserName })
            .ToListAsync();

        var items = users.Select(user => new UserDisplayName(user.Id, ToDisplayName(user.UserName!))).ToList();
        return new UserSearchResult(items, totalCount);
    }

    public async Task<List<UserDisplayName>> GetDisplayNamesAsync(List<Guid> userIds)
    {
        var users = await _dbContext.Users
            .Where(user => userIds.Contains(user.Id))
            .Select(user => new { user.Id, user.UserName })
            .ToListAsync();

        return users.Select(user => new UserDisplayName(user.Id, ToDisplayName(user.UserName!))).ToList();
    }

    public Task<bool> ExistsAsync(Guid userId)
    {
        return _dbContext.Users.AnyAsync(user => user.Id == userId);
    }

    // The user name is the e-mail address; other modules see only the part before the @.
    private static string ToDisplayName(string userName)
    {
        var at = userName.IndexOf('@');
        return at < 0 ? userName : userName[..at];
    }
}
