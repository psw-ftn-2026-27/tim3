using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;
using Social.Application.ClubManagement;

namespace Social.Api;

[ApiController]
[Route("api/social/clubs")]
[Authorize]
public sealed class ClubManagementController : ControllerBase
{
    private readonly ClubManagementService _managementService;
    private readonly ClubManagementQueries _managementQueries;

    public ClubManagementController(
        ClubManagementService managementService,
        ClubManagementQueries managementQueries)
    {
        _managementService = managementService;
        _managementQueries = managementQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<ClubDto>>> GetPage([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _managementQueries.GetPageAsync(page, pageSize);
    }

    [HttpPost]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult<Guid>> Create(CreateClubDto dto)
    {
        return await _managementService.CreateAsync(User.GetUserId(), dto);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> Update(Guid id, UpdateClubDto dto)
    {
        await _managementService.UpdateAsync(User.GetUserId(), id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _managementService.DeleteAsync(User.GetUserId(), id);
        return NoContent();
    }
}
