namespace Identity.Contracts;

public sealed record UserSearchResult(List<UserDisplayName> Items, int TotalCount);
