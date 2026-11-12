using Identity.Core;
using Identity.Core.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api;

[ApiController]
[Route("api/identity")]
public sealed class IdentityController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtTokenFactory _tokenFactory;

    public IdentityController(UserManager<ApplicationUser> userManager, JwtTokenFactory tokenFactory)
    {
        _userManager = userManager;
        _tokenFactory = tokenFactory;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenDto>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Invalid email or password.");
        }

        return new AccessTokenDto(_tokenFactory.CreateToken(user, await _userManager.GetRolesAsync(user)));
    }
}
