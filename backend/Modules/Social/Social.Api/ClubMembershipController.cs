using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Social.Application.ClubMembership;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubMembershipController : ControllerBase
{
    private readonly ClubMembershipService _membershipService;
    private readonly ClubMembershipQueries _membershipQueries;

    public ClubMembershipController(
        ClubMembershipService membershipService,
        ClubMembershipQueries membershipQueries)
    {
        _membershipService = membershipService;
        _membershipQueries = membershipQueries;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClubDetailsDto>> Get(Guid id)
    {
        return await _membershipQueries.GetClubAsync(User.GetUserId(), id);
    }

    [HttpPost("{id:guid}/join-requests")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> RequestToJoin(Guid id)
    {
        await _membershipService.RequestToJoinAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpDelete("{id:guid}/join-requests")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> WithdrawRequest(Guid id)
    {
        await _membershipService.WithdrawRequestAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("{id:guid}/join-requests/{requesterId:guid}/accept")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> AcceptRequest(Guid id, Guid requesterId)
    {
        await _membershipService.AcceptRequestAsync(User.GetUserId(), id, requesterId);
        return NoContent();
    }

    [HttpPost("{id:guid}/join-requests/{requesterId:guid}/reject")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> RejectRequest(Guid id, Guid requesterId)
    {
        await _membershipService.RejectRequestAsync(User.GetUserId(), id, requesterId);
        return NoContent();
    }
}
