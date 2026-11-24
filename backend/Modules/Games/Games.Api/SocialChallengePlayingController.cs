using Games.Application.SocialChallengePlaying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/social-challenges")]
[Authorize(Roles = "explorer")]
public sealed class SocialChallengePlayingController : ControllerBase
{
    private readonly SocialChallengePlayingService _playingService;
    private readonly SocialChallengePlayingQueries _playingQueries;

    public SocialChallengePlayingController(
        SocialChallengePlayingService playingService,
        SocialChallengePlayingQueries playingQueries)
    {
        _playingService = playingService;
        _playingQueries = playingQueries;
    }

    [HttpGet("published")]
    public async Task<ActionResult<PageResult<PublishedSocialChallengeDto>>> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _playingQueries.GetPublishedAsync(User.GetUserId(), page, pageSize);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id)
    {
        await _playingService.ActivateAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpPost("{id:guid}/position-reports")]
    public async Task<ActionResult> ReportPosition(Guid id)
    {
        await _playingService.ReportPositionAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{id:guid}/execution")]
    public async Task<ActionResult<SocialChallengeExecutionDto>> GetExecution(Guid id)
    {
        return await _playingQueries.GetExecutionAsync(id, User.GetUserId());
    }
}
