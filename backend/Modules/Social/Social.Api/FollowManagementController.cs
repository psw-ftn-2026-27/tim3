using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.FollowManagement;

namespace Social.Api;

[ApiController]
[Route("api/social")]
[Authorize(Roles = "explorer")]
public sealed class FollowManagementController : ControllerBase
{
    private readonly FollowManagementService _managementService;
    private readonly FollowManagementQueries _managementQueries;

    public FollowManagementController(
        FollowManagementService managementService,
        FollowManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpGet("profile/summary")]
    public async Task<ActionResult<ProfileSummaryDto>> GetProfileSummary()
    {
        return await _managementQueries.GetProfileSummaryAsync(User.GetUserId());
    }

    [HttpGet("profile/followers")]
    public async Task<ActionResult<PageResult<ConnectedUserDto>>> GetFollowers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetFollowersAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("profile/following")]
    public async Task<ActionResult<PageResult<ConnectedUserDto>>> GetFollowing(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetFollowingAsync(User.GetUserId(), page, pageSize);
    }

    [HttpDelete("profile/followers/{followerId:guid}")]
    public async Task<ActionResult> RemoveFollower(Guid followerId)
    {
        await _managementService.RemoveFollowerAsync(User.GetUserId(), followerId);
        return NoContent();
    }

    [HttpDelete("profile/following/{followedId:guid}")]
    public async Task<ActionResult> Unfollow(Guid followedId)
    {
        await _managementService.UnfollowAsync(User.GetUserId(), followedId);
        return NoContent();
    }
}
