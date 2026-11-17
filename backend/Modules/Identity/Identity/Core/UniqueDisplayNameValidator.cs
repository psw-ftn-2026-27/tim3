using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Core;

// Other modules know a user only by the display name, so two accounts with the same name
// would be indistinguishable there. Like RequireUniqueEmail, the check runs whenever
// UserManager creates or updates a user.
internal sealed class UniqueDisplayNameValidator : IUserValidator<ApplicationUser>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        var displayName = DisplayName.From(user.UserName ?? string.Empty);
        var normalizedDisplayName = displayName.ToUpperInvariant();
        var normalizedPrefix = normalizedDisplayName + "@";

        var taken = await manager.Users.AnyAsync(other =>
            other.Id != user.Id &&
            (other.NormalizedUserName == normalizedDisplayName ||
             other.NormalizedUserName!.StartsWith(normalizedPrefix)));

        return taken
            ? IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateDisplayName",
                Description = $"The name '{displayName}' is already taken. Use an e-mail address with a different part before the @."
            })
            : IdentityResult.Success;
    }
}
