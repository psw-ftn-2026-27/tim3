namespace Identity.Contracts;

// The only part of Identity other modules may see. A user appears here as an ID and a
// display name; the e-mail address, password and roles never leave Identity.
public interface IIdentityApi
{
    Task<UserSearchResult> SearchUsersAsync(string text, Guid excludedUserId, int page, int pageSize);

    Task<UserSearchResult> SearchAllUsersAsync(string text, int page, int pageSize);

    Task<List<UserDisplayName>> GetDisplayNamesAsync(List<Guid> userIds);

    Task<bool> ExistsAsync(Guid userId);
}
