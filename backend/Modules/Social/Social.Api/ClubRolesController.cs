using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubRoles;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize(Roles = "explorer")]
public sealed class ClubRolesController : ControllerBase
{
    private readonly ClubRolesService _rolesService;

    public ClubRolesController(ClubRolesService rolesService)
    {
        _rolesService = rolesService;
    }

    [HttpPost("{id:guid}/members/{memberId:guid}/promote")]
    public async Task<ActionResult> PromoteToModerator(Guid id, Guid memberId)
    {
        await _rolesService.PromoteToModeratorAsync(User.GetUserId(), id, memberId);
        return NoContent();
    }

    [HttpPost("{id:guid}/members/{newOwnerId:guid}/transfer-ownership")]
    public async Task<ActionResult> TransferOwnership(Guid id, Guid newOwnerId)
    {
        await _rolesService.TransferOwnershipAsync(User.GetUserId(), id, newOwnerId);
        return NoContent();
    }
}
