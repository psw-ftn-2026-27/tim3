using Identity.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;

internal sealed class IdentityApi : IIdentityApi
{
    private const string ExplorerRole = "explorer";

    private readonly IdentityModuleDbContext _dbContext;

    public IdentityApi(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserSearchResult> SearchUsersAsync(string text, Guid excludedUserId, int page, int pageSize)
    {
        // Only explorers take part in the social features, and only the display name (the part
        // of the address before the @) is matched, so a search never reveals e-mail domains.
        var normalizedText = text.Trim().ToUpperInvariant();
        var explorerRoleIds = _dbContext.Roles
            .Where(role => role.Name == ExplorerRole)
            .Select(role => role.Id);
        var matching = _dbContext.Users
            .Where(user => user.Id != excludedUserId)
            .Where(user => _dbContext.UserRoles.Any(userRole =>
                userRole.UserId == user.Id && explorerRoleIds.Contains(userRole.RoleId)))
            .Where(user => user.NormalizedUserName!
                .Substring(0, user.NormalizedUserName.IndexOf("@"))
                .Contains(normalizedText));

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

    public async Task<UserSearchResult> SearchAllUsersAsync(string text, int page, int pageSize)
    {
        var normalizedText = text.Trim().ToUpperInvariant();
        var matching = _dbContext.Users.AsNoTracking();
        if (normalizedText.Length > 0)
        {
            matching = matching.Where(user => user.NormalizedUserName != null &&
                (user.NormalizedUserName.Contains("@")
                    ? user.NormalizedUserName.Substring(0, user.NormalizedUserName.IndexOf("@"))
                    : user.NormalizedUserName).Contains(normalizedText));
        }

        var totalCount = await matching.CountAsync();
        var offset = (int)Math.Min((page - 1L) * pageSize, int.MaxValue);
        var users = await matching
            .OrderBy(user => user.NormalizedUserName)
            .ThenBy(user => user.Id)
            .Skip(offset)
            .Take(pageSize)
            .Select(user => new { user.Id, user.UserName })
            .ToListAsync();

        var items = users.Select(user => new UserDisplayName(user.Id, ToDisplayName(user.UserName ?? string.Empty)))
            .ToList();
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
