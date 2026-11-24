using Games.Application.SocialChallengeAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Route("api/games/social-challenges")]
[Authorize(Roles = "administrator")]
public sealed class SocialChallengeAuthoringController : ControllerBase
{
    private readonly SocialChallengeAuthoringService _authoringService;
    private readonly SocialChallengeAuthoringQueries _authoringQueries;

    public SocialChallengeAuthoringController(
        SocialChallengeAuthoringService authoringService,
        SocialChallengeAuthoringQueries authoringQueries)
    {
        _authoringService = authoringService;
        _authoringQueries = authoringQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<SocialChallengeDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _authoringQueries.GetAllAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SocialChallengeDto>> GetById(Guid id)
    {
        return await _authoringQueries.GetByIdAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<SocialChallengeDto>> Create(CreateSocialChallengeDto dto)
    {
        return await _authoringService.CreateAsync(dto);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateSocialChallengeDto dto)
    {
        await _authoringService.UpdateAsync(id, dto);
        return NoContent();
    }
}
