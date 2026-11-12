using Games.Application.QuizLifecycle;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "administrator")]
public sealed class QuizLifecycleController : ControllerBase
{
    private readonly QuizLifecycleService _lifecycleService;

    public QuizLifecycleController(QuizLifecycleService lifecycleService)
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
