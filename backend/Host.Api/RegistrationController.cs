using Identity.Core;
using Identity.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Host.Api;

[ApiController]
[Route("api/identity")]
public sealed class RegistrationController : ControllerBase
{
    private readonly IdentityRegistrationService _registrationService;
    private readonly RegistrationWalletProvisioning _walletProvisioning;

    public RegistrationController(
        IdentityRegistrationService registrationService, RegistrationWalletProvisioning walletProvisioning)
    {
        _registrationService = registrationService;
        _walletProvisioning = walletProvisioning;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AccessTokenDto>> Register(RegisterDto dto)
    {
        var (userId, rejectionReason) = await _registrationService.RegisterAsync(dto);
        if (userId is not Guid registeredUserId)
        {
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: rejectionReason);
        }

        await _walletProvisioning.EnsureAsync(registeredUserId);
        return await _registrationService.CreateAccessTokenAsync(registeredUserId);
    }
}
