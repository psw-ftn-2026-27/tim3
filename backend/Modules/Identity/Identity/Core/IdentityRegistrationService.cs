using Identity.Core.DTOs;
using Microsoft.AspNetCore.Identity;

namespace Identity.Core;

public sealed class IdentityRegistrationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenFactory _tokenFactory;

    public IdentityRegistrationService(UserManager<ApplicationUser> userManager, JwtTokenFactory tokenFactory)
    {
        _userManager = userManager;
        _tokenFactory = tokenFactory;
    }

    public async Task<(Guid? UserId, string? RejectionReason)> RegisterAsync(RegisterDto dto)
    {
        var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var reason = string.Join(" ", result.Errors.Select(error => error.Description));
            return (null, reason);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, "explorer");
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException("Could not assign the explorer role to a registered user.");
        }

        return (user.Id, null);
    }

    public async Task<AccessTokenDto> CreateAccessTokenAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("The registered user no longer exists.");
        return new AccessTokenDto(_tokenFactory.CreateToken(user, await _userManager.GetRolesAsync(user)));
    }
}
