using Games.Application.QuizAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/quizzes")]
[Authorize(Roles = "administrator")]
public sealed class QuizAuthoringController : ControllerBase
{
    private readonly QuizAuthoringService _authoringService;
    private readonly QuizAuthoringQueries _authoringQueries;

    public QuizAuthoringController(
        QuizAuthoringService authoringService,
        QuizAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateQuizDto dto)
    {
        return await _authoringService.CreateAsync(User.GetUserId(), dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateDetails(Guid id, UpdateQuizDetailsDto dto)
    {
        await _authoringService.UpdateDetailsAsync(id, dto);
        return NoContent();
    }

    [HttpPost("{id:guid}/questions")]
    public async Task<ActionResult> AddQuestion(Guid id, CreateQuestionDto dto)
    {
        await _authoringService.AddQuestionAsync(id, dto);
        return NoContent();
    }

    [HttpPut("{id:guid}/questions/{questionId:guid}")]
    public async Task<ActionResult> UpdateQuestion(Guid id, Guid questionId, UpdateQuestionDto dto)
    {
        await _authoringService.UpdateQuestionAsync(id, questionId, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    public async Task<ActionResult> RemoveQuestion(Guid id, Guid questionId)
    {
        await _authoringService.RemoveQuestionAsync(id, questionId);
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<QuizDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _authoringQueries.GetAllAsync(page, pageSize);
    }

    [HttpGet("{id:guid}/edit")]
    public async Task<ActionResult<QuizDto>> GetForEdit(Guid id)
    {
        return await _authoringQueries.GetByIdAsync(id);
    }
}
