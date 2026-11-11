using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.FollowRequesting;

namespace Social.Api;

[ApiController]
[Route("api/social")]
[Authorize(Roles = "explorer")]
public sealed class FollowRequestingController : ControllerBase
{
    private readonly FollowRequestingService _requestingService;
    private readonly FollowRequestingQueries _requestingQueries;

    public FollowRequestingController(
        FollowRequestingService requestingService,
        FollowRequestingQueries requestingQueries)
    {
        _requestingService = requestingService;
        _requestingQueries = requestingQueries;
    }

    [HttpGet("users")]
    public async Task<ActionResult<PageResult<FoundUserDto>>> SearchUsers(
        [FromQuery] string text, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _requestingQueries.SearchUsersAsync(User.GetUserId(), text, page, pageSize);
    }

    [HttpGet("follow-requests/sent")]
    public async Task<ActionResult<PageResult<SentFollowRequestDto>>> GetSent(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _requestingQueries.GetSentRequestsAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("follow-requests/incoming")]
    public async Task<ActionResult<PageResult<IncomingFollowRequestDto>>> GetIncoming(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _requestingQueries.GetIncomingRequestsAsync(User.GetUserId(), page, pageSize);
    }

    [HttpPost("follow-requests")]
    public async Task<ActionResult> Send(SendFollowRequestDto dto)
    {
        await _requestingService.SendAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete("follow-requests/{id:guid}")]
    public async Task<ActionResult> Cancel(Guid id)
    {
        await _requestingService.CancelAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("follow-requests/{id:guid}/accept")]
    public async Task<ActionResult> Accept(Guid id)
    {
        await _requestingService.AcceptAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpPost("follow-requests/{id:guid}/reject")]
    public async Task<ActionResult> Reject(Guid id)
    {
        await _requestingService.RejectAsync(User.GetUserId(), id);
        return NoContent();
    }
}
