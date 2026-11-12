using Games.Application.SocialChallengeLifecycle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/social-challenges")]
[Authorize(Roles = "administrator")]
public sealed class SocialChallengeLifecycleController : ControllerBase
{
    private readonly SocialChallengeLifecycleService _lifecycleService;

    public SocialChallengeLifecycleController(SocialChallengeLifecycleService lifecycleService)
    {
        _lifecycleService = lifecycleService;
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult> Publish(Guid id)
    {
        await _lifecycleService.PublishAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/retire")]
    public async Task<ActionResult> Retire(Guid id)
    {
        await _lifecycleService.RetireAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    public async Task<ActionResult> Reactivate(Guid id)
    {
        await _lifecycleService.ReactivateAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult> Archive(Guid id)
    {
        await _lifecycleService.ArchiveAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _lifecycleService.DeleteAsync(id);
        return NoContent();
    }
}
