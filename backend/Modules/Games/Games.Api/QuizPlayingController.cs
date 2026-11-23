using Games.Application.QuizPlaying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "explorer")]
public sealed class QuizPlayingController : ControllerBase
{
    private readonly QuizPlayingService _playingService;
    private readonly QuizPlayingQueries _playingQueries;

    public QuizPlayingController(
        QuizPlayingService playingService,
        QuizPlayingQueries playingQueries)
    {
        _playingService = playingService;
        _playingQueries = playingQueries;
    }

    [HttpGet("published")]
    public async Task<ActionResult<PageResult<PublishedQuizDto>>> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _playingQueries.GetPublishedAsync(User.GetUserId(), page, pageSize);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult> Activate(Guid id)
    {
        await _playingService.ActivateAsync(id, User.GetUserId());
        return NoContent();
    }

    [HttpGet("{id:guid}/play")]
    public async Task<ActionResult<QuizPlayDto>> GetPlay(Guid id)
    {
        return await _playingQueries.GetPlayAsync(id, User.GetUserId());
    }

    [HttpPost("{id:guid}/answers")]
    public async Task<ActionResult> SubmitAnswer(Guid id, SubmitAnswerDto dto)
    {
        await _playingService.SubmitAnswerAsync(id, User.GetUserId(), dto);
        return NoContent();
    }
}
